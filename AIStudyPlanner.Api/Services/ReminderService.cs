using AIStudyPlanner.Api.Data;
using AIStudyPlanner.Api.DTOs.Reminders;
using AIStudyPlanner.Api.Entities;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AIStudyPlanner.Api.Services;

public class ReminderService(
    ApplicationDbContext dbContext,
    IEmailService emailService,
    ILogger<ReminderService> logger) : IReminderService
{
    public async Task<IReadOnlyCollection<ReminderResponse>> GetRemindersAsync(Guid userId)
    {
        var reminders = await dbContext.Reminders
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.ReminderDateTime)
            .ToListAsync();

        return reminders.Select(Map).ToList();
    }

    public async Task<ReminderResponse> CreateReminderAsync(Guid userId, ReminderCreateRequest request)
    {
        if (request.StudyTaskId.HasValue &&
            !await dbContext.StudyTasks.AnyAsync(x => x.UserId == userId && x.Id == request.StudyTaskId.Value))
        {
            throw new KeyNotFoundException("Task not found.");
        }

        var reminder = new Reminder
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StudyTaskId = request.StudyTaskId,
            Title = request.Title.Trim(),
            Message = request.Message.Trim(),
            ReminderDateTime = request.ReminderDateTime,
            Channel = request.Channel,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Reminders.Add(reminder);
        await dbContext.SaveChangesAsync();
        return Map(reminder);
    }

    public async Task<ReminderResponse> UpdateReminderAsync(Guid userId, Guid reminderId, ReminderUpdateRequest request)
    {
        var reminder = await dbContext.Reminders.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == reminderId)
            ?? throw new KeyNotFoundException("Reminder not found.");

        reminder.StudyTaskId = request.StudyTaskId;
        reminder.Title = request.Title.Trim();
        reminder.Message = request.Message.Trim();
        reminder.ReminderDateTime = request.ReminderDateTime;
        reminder.Channel = request.Channel;
        reminder.IsSent = request.IsSent;
        reminder.IsRead = request.IsRead;
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
        var due = await dbContext.Reminders
            .Include(x => x.User)
            .Where(x => !x.IsSent && x.ReminderDateTime <= DateTime.UtcNow.AddMinutes(1))
            .OrderBy(x => x.ReminderDateTime)
            .ToListAsync(cancellationToken);

        foreach (var reminder in due)
        {
            if (reminder.Channel == ReminderChannel.Email && reminder.User is not null)
            {
                try
                {
                    await emailService.SendReminderAsync(reminder.User.Email, reminder.Title, reminder.Message, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Skipping email reminder {ReminderId} because SMTP delivery failed or is not configured.", reminder.Id);
                }
            }

            reminder.IsSent = true;
            reminder.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return due.Count;
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
        Channel = reminder.Channel
    };
}
