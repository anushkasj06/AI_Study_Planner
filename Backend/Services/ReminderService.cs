using AIStudyPlanner.Api.Data;
using AIStudyPlanner.Api.DTOs.Reminders;
using AIStudyPlanner.Api.Entities;
using AIStudyPlanner.Api.Helpers;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AIStudyPlanner.Api.Services;

public class ReminderService(
    ApplicationDbContext dbContext,
    IEmailService emailService,
    INotificationService notificationService,
    IWebPushService webPushService,
    IOptions<SmtpOptions> smtpOptions,
    ILogger<ReminderService> logger) : IReminderService
{
    private const int MaxDeliveryAttempts = 5;
    private static readonly TimeSpan DeliveryRetryBackoff = TimeSpan.FromMinutes(3);
    private readonly SmtpOptions _smtpOptions = smtpOptions.Value;

    public async Task<IReadOnlyCollection<ReminderResponse>> GetRemindersAsync(Guid userId)
    {
        var reminders = await dbContext.Reminders
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.ReminderDateTime)
            .ToListAsync();

        return reminders.Select(Map).ToList();
    }

    public async Task<ReminderResponse> CreateReminderAsync(Guid userId, ReminderCreateRequest request)
    {
        var created = await CreateReminderBatchInternalAsync(
            userId,
            new ReminderBatchCreateRequest
            {
                StudyTaskId = request.StudyTaskId,
                Title = request.Title,
                Message = request.Message,
                ReminderDateTime = request.ReminderDateTime,
                Channels = [request.Channel]
            },
            emitSchedulingNotification: true,
            CancellationToken.None);

        return created.First();
    }

    public async Task<IReadOnlyCollection<ReminderResponse>> CreateReminderBatchAsync(
        Guid userId,
        ReminderBatchCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        return await CreateReminderBatchInternalAsync(userId, request, emitSchedulingNotification: true, cancellationToken);
    }

    public async Task<AiReminderDraftResponse> GenerateAiReminderDraftAsync(
        Guid userId,
        AiReminderDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        StudyTask? task = null;
        StudyGoal? goal = null;

        if (request.StudyTaskId.HasValue)
        {
            task = await dbContext.StudyTasks
                .Include(x => x.StudyGoal)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == request.StudyTaskId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Task not found.");

            goal = task.StudyGoal;
        }

        if (request.StudyGoalId.HasValue && goal is null)
        {
            goal = await dbContext.StudyGoals
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == request.StudyGoalId.Value, cancellationToken)
                ?? throw new KeyNotFoundException("Goal not found.");
        }

        return await BuildAiReminderDraftAsync(userId, task, goal, request, cancellationToken);
    }

    public Task<int> GenerateSmartRemindersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return GenerateSmartRemindersForScopeAsync(userId, cancellationToken);
    }

    public async Task<ReminderResponse> UpdateReminderAsync(Guid userId, Guid reminderId, ReminderUpdateRequest request)
    {
        if (request.StudyTaskId.HasValue &&
            !await dbContext.StudyTasks.AnyAsync(x => x.UserId == userId && x.Id == request.StudyTaskId.Value))
        {
            throw new KeyNotFoundException("Task not found.");
        }

        var reminder = await dbContext.Reminders.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == reminderId)
            ?? throw new KeyNotFoundException("Reminder not found.");

        var normalizedReminderTime = NormalizeToUtc(request.ReminderDateTime);
        var requiresRedelivery = reminder.ReminderDateTime != normalizedReminderTime ||
                                 reminder.Channel != request.Channel ||
                                 !string.Equals(reminder.Title, request.Title.Trim(), StringComparison.Ordinal) ||
                                 !string.Equals(reminder.Message, request.Message.Trim(), StringComparison.Ordinal);

        reminder.StudyTaskId = request.StudyTaskId;
        reminder.Title = request.Title.Trim();
        reminder.Message = request.Message.Trim();
        reminder.ReminderDateTime = normalizedReminderTime;
        reminder.Channel = request.Channel;
        reminder.IsRead = request.IsRead;

        if (requiresRedelivery)
        {
            reminder.IsSent = false;
            reminder.DeliveryStatus = ReminderDeliveryStatus.Pending;
            reminder.DeliveryAttempts = 0;
            reminder.LastDeliveryAttemptAtUtc = null;
            reminder.LastDeliveryError = string.Empty;
        }

        reminder.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return Map(reminder);
    }

    public async Task DeleteReminderAsync(Guid userId, Guid reminderId)
    {
        var reminder = await dbContext.Reminders.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == reminderId)
            ?? throw new KeyNotFoundException("Reminder not found.");

        dbContext.Reminders.Remove(reminder);
        await dbContext.SaveChangesAsync();
    }

    public async Task<ReminderResponse> MarkReadAsync(Guid userId, Guid reminderId)
    {
        var reminder = await dbContext.Reminders.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == reminderId)
            ?? throw new KeyNotFoundException("Reminder not found.");

        reminder.IsRead = true;
        reminder.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
        return Map(reminder);
    }

    public async Task<int> ProcessDueRemindersAsync(CancellationToken cancellationToken = default)
    {
        await GenerateSmartRemindersForScopeAsync(null, cancellationToken);
        return await ProcessDueRemindersInternalAsync(null, cancellationToken);
    }

    public async Task<int> ProcessDueRemindersForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await GenerateSmartRemindersForScopeAsync(userId, cancellationToken);
        return await ProcessDueRemindersInternalAsync(userId, cancellationToken);
    }

    private async Task<IReadOnlyCollection<ReminderResponse>> CreateReminderBatchInternalAsync(
        Guid userId,
        ReminderBatchCreateRequest request,
        bool emitSchedulingNotification,
        CancellationToken cancellationToken)
    {
        if (request.StudyTaskId.HasValue &&
            !await dbContext.StudyTasks.AnyAsync(x => x.UserId == userId && x.Id == request.StudyTaskId.Value, cancellationToken))
        {
            throw new KeyNotFoundException("Task not found.");
        }

        var channels = request.Channels
            .Where(x => Enum.IsDefined(x))
            .Distinct()
            .ToList();

        if (channels.Count == 0)
        {
            channels.Add(ReminderChannel.InApp);
        }

        var normalizedReminderTime = NormalizeToUtc(request.ReminderDateTime);
        var title = request.Title.Trim();
        var message = request.Message.Trim();
        var reminders = new List<Reminder>();

        foreach (var channel in channels)
        {
            reminders.Add(new Reminder
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StudyTaskId = request.StudyTaskId,
                Title = title,
                Message = message,
                ReminderDateTime = normalizedReminderTime,
                Channel = channel,
                IsSent = false,
                IsRead = false,
                DeliveryStatus = ReminderDeliveryStatus.Pending,
                DeliveryAttempts = 0,
                LastDeliveryAttemptAtUtc = null,
                LastDeliveryError = string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        dbContext.Reminders.AddRange(reminders);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (emitSchedulingNotification)
        {
            await notificationService.CreateAsync(
                userId,
                "Reminder scheduled",
                $"{title} at {normalizedReminderTime:dd MMM yyyy HH:mm} UTC via {string.Join(", ", channels)}.",
                NotificationType.Reminder,
                reminders[0].Id,
                cancellationToken);
        }

        return reminders.Select(Map).ToList();
    }

    private async Task<int> GenerateSmartRemindersForScopeAsync(Guid? userId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var horizonDate = now.Date.AddDays(3);

        var taskQuery = dbContext.StudyTasks
            .Include(x => x.StudyGoal)
            .Where(x => !x.IsCompleted && x.TaskDate >= now.Date && x.TaskDate <= horizonDate);

        if (userId.HasValue)
        {
            taskQuery = taskQuery.Where(x => x.UserId == userId.Value);
        }

        var upcomingTasks = await taskQuery
            .OrderBy(x => x.TaskDate)
            .Take(120)
            .ToListAsync(cancellationToken);

        var created = 0;

        foreach (var task in upcomingTasks)
        {
            var reminderExists = await dbContext.Reminders
                .AnyAsync(
                    x => x.UserId == task.UserId &&
                         x.StudyTaskId == task.Id &&
                         x.ReminderDateTime >= now.AddHours(-1),
                    cancellationToken);

            if (reminderExists)
            {
                continue;
            }

            var draft = await BuildAiReminderDraftAsync(
                task.UserId,
                task,
                task.StudyGoal,
                new AiReminderDraftRequest
                {
                    StudyTaskId = task.Id,
                    Prompt = string.Empty,
                    PreferBrowserPush = true,
                    PreferEmail = false
                },
                cancellationToken);

            var createdReminders = await CreateReminderBatchInternalAsync(
                task.UserId,
                new ReminderBatchCreateRequest
                {
                    StudyTaskId = task.Id,
                    Title = draft.Title,
                    Message = draft.Message,
                    ReminderDateTime = draft.ReminderDateTime,
                    Channels = draft.RecommendedChannels.ToList()
                },
                emitSchedulingNotification: false,
                cancellationToken);

            created += createdReminders.Count;
        }

        if (created > 0)
        {
            logger.LogInformation("AI reminder sync created {Count} reminder records.", created);
        }

        return created;
    }

    private async Task<int> ProcessDueRemindersInternalAsync(Guid? userId, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var dueQuery = dbContext.Reminders
            .Include(x => x.User)
            .Where(x => !x.IsSent && x.ReminderDateTime <= now.AddMinutes(1) && x.DeliveryAttempts < MaxDeliveryAttempts);

        if (userId.HasValue)
        {
            dueQuery = dueQuery.Where(x => x.UserId == userId.Value);
        }

        var due = await dueQuery
            .OrderBy(x => x.ReminderDateTime)
            .Take(200)
            .ToListAsync(cancellationToken);

        var processed = 0;

        foreach (var reminder in due)
        {
            if (reminder.LastDeliveryAttemptAtUtc.HasValue &&
                reminder.LastDeliveryAttemptAtUtc.Value > now.Subtract(DeliveryRetryBackoff))
            {
                continue;
            }

            processed += 1;
            reminder.DeliveryAttempts += 1;
            reminder.LastDeliveryAttemptAtUtc = now;

            var deliveryResult = await DeliverReminderAsync(reminder, cancellationToken);

            if (deliveryResult.Success)
            {
                await notificationService.CreateAsync(
                    reminder.UserId,
                    reminder.Title,
                    reminder.Message,
                    NotificationType.Reminder,
                    reminder.Id,
                    cancellationToken);

                reminder.IsSent = true;
                reminder.DeliveryStatus = ReminderDeliveryStatus.Sent;
                reminder.LastDeliveryError = string.Empty;
            }
            else
            {
                reminder.IsSent = false;
                reminder.DeliveryStatus = ReminderDeliveryStatus.Failed;
                reminder.LastDeliveryError = Truncate(deliveryResult.Message, 800);

                await NotifyDeliveryFailureAsync(reminder, deliveryResult.Message, cancellationToken);

                if (reminder.DeliveryAttempts >= MaxDeliveryAttempts)
                {
                    await notificationService.CreateAsync(
                        reminder.UserId,
                        "Reminder stopped after retries",
                        $"{reminder.Title}: {Truncate(deliveryResult.Message, 220)}",
                        NotificationType.System,
                        reminder.Id,
                        cancellationToken);
                }
            }

            reminder.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return processed;
    }

    private async Task<DeliveryResult> DeliverReminderAsync(Reminder reminder, CancellationToken cancellationToken)
    {
        try
        {
            if (reminder.Channel == ReminderChannel.InApp)
            {
                return DeliveryResult.Ok("In-app reminder is ready.");
            }

            if (reminder.Channel == ReminderChannel.Email)
            {
                if (reminder.User is null || string.IsNullOrWhiteSpace(reminder.User.Email))
                {
                    return DeliveryResult.Fail("User email is unavailable.");
                }

                return await emailService.SendReminderAsync(
                    reminder.User.Email,
                    reminder.Title,
                    reminder.Message,
                    cancellationToken);
            }

            if (reminder.Channel == ReminderChannel.BrowserPush)
            {
                return await webPushService.SendToUserAsync(reminder.UserId, reminder.Title, reminder.Message, cancellationToken);
            }

            return DeliveryResult.Fail("Unsupported reminder channel.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Delivery pipeline failed for reminder {ReminderId}.", reminder.Id);
            return DeliveryResult.Fail($"Delivery failed: {ex.Message}");
        }
    }

    private async Task NotifyDeliveryFailureAsync(Reminder reminder, string errorMessage, CancellationToken cancellationToken)
    {
        var hasRecentFailureNote = await dbContext.UserNotifications
            .AnyAsync(
                x => x.UserId == reminder.UserId &&
                     x.ReminderId == reminder.Id &&
                     x.Type == NotificationType.System &&
                     x.Title == "Reminder delivery issue" &&
                     x.CreatedAt >= DateTime.UtcNow.AddHours(-3),
                cancellationToken);

        if (hasRecentFailureNote)
        {
            return;
        }

        await notificationService.CreateAsync(
            reminder.UserId,
            "Reminder delivery issue",
            $"{reminder.Title}: {Truncate(errorMessage, 220)}",
            NotificationType.System,
            reminder.Id,
            cancellationToken);
    }

    private async Task<AiReminderDraftResponse> BuildAiReminderDraftAsync(
        Guid userId,
        StudyTask? task,
        StudyGoal? goal,
        AiReminderDraftRequest request,
        CancellationToken cancellationToken)
    {
        var signals = await GetUserSignalsAsync(userId, cancellationToken);
        var now = DateTime.UtcNow;

        var preferredTime = request.PreferredReminderDateTime.HasValue
            ? NormalizeToUtc(request.PreferredReminderDateTime.Value)
            : BuildTaskReminderUtc(task, goal, now);

        if (preferredTime < now.AddMinutes(5))
        {
            preferredTime = now.AddMinutes(5);
        }

        var focusTopic = task is null
            ? "your highest priority topic"
            : $"{task.Topic} ({task.Subtopic})";

        var effortHours = task?.EstimatedHours ?? 1.5m;
        var momentumLabel = signals.CompletionRate >= 70m
            ? "strong"
            : signals.CompletionRate >= 40m
                ? "steady"
                : "recovering";

        var coachingTip = signals.CompletionRate < 40m
            ? "Start with a 15-minute warm-up and finish with a short recap."
            : "Start with your hardest subtopic first and end with active recall.";

        var promptFragment = string.IsNullOrWhiteSpace(request.Prompt)
            ? string.Empty
            : $" {Truncate(request.Prompt.Trim(), 90)}";

        var title = Truncate($"AI reminder: {focusTopic}{promptFragment}", 150);
        var message = Truncate(
            $"Momentum is {momentumLabel}. Focus on {focusTopic} for about {effortHours:0.#}h. {coachingTip}",
            500);

        var channels = new List<ReminderChannel> { ReminderChannel.InApp };
        if ((request.PreferBrowserPush || signals.UpcomingTaskCount > 0) && signals.HasPushSubscription)
        {
            channels.Add(ReminderChannel.BrowserPush);
        }

        if ((request.PreferEmail || signals.CompletionRate < 45m) && _smtpOptions.IsConfigured)
        {
            channels.Add(ReminderChannel.Email);
        }

        var reasoning =
            $"Signals: {signals.RecentHoursLast7Days:0.#}h logged in 7d, {signals.CompletionRate:0.#}% completion, {signals.UpcomingTaskCount} upcoming tasks, push subscription {(signals.HasPushSubscription ? "available" : "missing")}, SMTP {(_smtpOptions.IsConfigured ? "configured" : "not configured")}.";

        return new AiReminderDraftResponse
        {
            Title = title,
            Message = message,
            ReminderDateTime = preferredTime,
            RecommendedChannels = channels.Distinct().ToList(),
            Reasoning = reasoning
        };
    }

    private async Task<UserSignals> GetUserSignalsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-7);

        var recentHours = await dbContext.ProgressLogs
            .Where(x => x.UserId == userId && x.Date >= sevenDaysAgo)
            .SumAsync(x => (decimal?)x.HoursSpent, cancellationToken) ?? 0m;

        var totalTasks = await dbContext.StudyTasks
            .CountAsync(x => x.UserId == userId, cancellationToken);

        var completedTasks = await dbContext.StudyTasks
            .CountAsync(x => x.UserId == userId && x.IsCompleted, cancellationToken);

        var upcomingTaskCount = await dbContext.StudyTasks
            .CountAsync(
                x => x.UserId == userId &&
                     !x.IsCompleted &&
                     x.TaskDate >= DateTime.UtcNow.Date &&
                     x.TaskDate <= DateTime.UtcNow.Date.AddDays(3),
                cancellationToken);

        var hasPushSubscription = await dbContext.WebPushSubscriptions
            .AnyAsync(x => x.UserId == userId, cancellationToken);

        var completionRate = totalTasks == 0 ? 0m : decimal.Round((decimal)completedTasks / totalTasks * 100m, 2);

        return new UserSignals(recentHours, completionRate, upcomingTaskCount, hasPushSubscription);
    }

    private static DateTime BuildTaskReminderUtc(StudyTask? task, StudyGoal? goal, DateTime now)
    {
        if (task is null)
        {
            return now.AddHours(2);
        }

        var preferredHour = goal?.PreferredStudyTime switch
        {
            PreferredStudyTime.Morning => 8,
            PreferredStudyTime.Afternoon => 14,
            PreferredStudyTime.Evening => 19,
            _ => 18
        };

        var taskDateUtc = DateTime.SpecifyKind(task.TaskDate.Date, DateTimeKind.Utc);
        var reminderTime = taskDateUtc.AddHours(preferredHour);

        if (reminderTime < now.AddMinutes(5))
        {
            reminderTime = now.AddMinutes(5);
        }

        return reminderTime;
    }

    private static DateTime NormalizeToUtc(DateTime input)
    {
        return input.Kind switch
        {
            DateTimeKind.Utc => input,
            DateTimeKind.Local => input.ToUniversalTime(),
            _ => DateTime.SpecifyKind(input, DateTimeKind.Local).ToUniversalTime()
        };
    }

    private static ReminderResponse Map(Reminder reminder) => new()
    {
        Id = reminder.Id,
        StudyTaskId = reminder.StudyTaskId,
        Title = reminder.Title,
        Message = reminder.Message,
        ReminderDateTime = reminder.ReminderDateTime,
        IsSent = reminder.IsSent,
        IsRead = reminder.IsRead,
        Channel = reminder.Channel,
        DeliveryStatus = reminder.DeliveryStatus,
        DeliveryAttempts = reminder.DeliveryAttempts,
        LastDeliveryAttemptAtUtc = reminder.LastDeliveryAttemptAtUtc,
        LastDeliveryError = reminder.LastDeliveryError
    };

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }

    private sealed record UserSignals(
        decimal RecentHoursLast7Days,
        decimal CompletionRate,
        int UpcomingTaskCount,
        bool HasPushSubscription);
}
