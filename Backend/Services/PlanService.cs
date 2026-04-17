using AIStudyPlanner.Api.Data;
using AIStudyPlanner.Api.DTOs.Plans;
using AIStudyPlanner.Api.DTOs.Tasks;
using AIStudyPlanner.Api.Entities;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AIStudyPlanner.Api.Services;

public class PlanService(
    ApplicationDbContext dbContext,
    IGeminiService geminiService,
    ILogger<PlanService> logger) : IPlanService
{
    public async Task<AiProviderStatusResponse> GetLatestProviderStatusAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var latestRequest = await dbContext.AiRequestLogs
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestRequest is null)
        {
            return new AiProviderStatusResponse
            {
                Provider = "None",
                RequestStatus = "None",
                UsedFallback = false,
                Message = "No AI plan generation attempts yet."
            };
        }

        var provider = InferProvider(latestRequest);

        return new AiProviderStatusResponse
        {
            Provider = provider,
            RequestStatus = latestRequest.Status.ToString(),
            UsedFallback = latestRequest.Status == AiRequestStatus.Fallback,
            StudyGoalId = latestRequest.StudyGoalId,
            LastAttemptAtUtc = latestRequest.CreatedAt,
            Message = BuildProviderStatusMessage(latestRequest, provider)
        };
    }

    public async Task<StudyPlanResponse> GeneratePlanAsync(Guid userId, Guid goalId, bool forceRegenerate, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.FindAsync([userId], cancellationToken)
            ?? throw new KeyNotFoundException("User not found.");
        var goal = await dbContext.StudyGoals.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == goalId, cancellationToken)
            ?? throw new KeyNotFoundException("Goal not found.");

        if (forceRegenerate)
        {
            var existingPlans = await dbContext.StudyPlans
                .Where(x => x.UserId == userId && x.StudyGoalId == goalId)
                .Include(x => x.Tasks)
                .ToListAsync(cancellationToken);

            if (existingPlans.Count > 0)
            {
                dbContext.StudyPlans.RemoveRange(existingPlans);
                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            var existingPlan = await dbContext.StudyPlans
                .Include(x => x.Tasks)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.StudyGoalId == goalId, cancellationToken);

            if (existingPlan is not null)
            {
                return MapPlan(existingPlan);
            }
        }

        GeneratedPlanResult generated;
        try
        {
            var planningContext = await BuildPlanningContextAsync(userId, goalId, cancellationToken);
            generated = await geminiService.GenerateStudyPlanAsync(user, goal, planningContext, cancellationToken);
            dbContext.AiRequestLogs.Add(new AiRequestLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StudyGoalId = goalId,
                Prompt = generated.RawPrompt,
                Response = generated.RawResponse,
                Status = generated.UsedFallback ? AiRequestStatus.Fallback : AiRequestStatus.Success,
                CreatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Plan generation failed for goal {GoalId}", goalId);
            dbContext.AiRequestLogs.Add(new AiRequestLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StudyGoalId = goalId,
                Prompt = goal.Title,
                Response = string.Empty,
                Status = AiRequestStatus.Failed,
                ErrorMessage = ex.Message,
                CreatedAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync(cancellationToken);
            throw;
        }

        var plan = new StudyPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StudyGoalId = goalId,
            Title = generated.PlanTitle,
            StartDate = generated.StartDate.Date,
            EndDate = generated.EndDate.Date,
            TotalEstimatedHours = generated.TotalEstimatedHours,
            GeneratedByAI = !generated.UsedFallback,
            RawAiPrompt = generated.RawPrompt,
            RawAiResponse = generated.RawResponse,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        foreach (var item in generated.Tasks)
        {
            if (!Enum.TryParse<TaskType>(item.TaskType, true, out var taskType))
            {
                taskType = TaskType.Learn;
            }

            if (!Enum.TryParse<GoalPriority>(item.Priority, true, out var priority))
            {
                priority = goal.Priority;
            }

            plan.Tasks.Add(new StudyTask
            {
                Id = Guid.NewGuid(),
                StudyPlanId = plan.Id,
                StudyGoalId = goalId,
                UserId = userId,
                TaskDate = item.TaskDate.Date,
                Topic = item.Topic.Trim(),
                Subtopic = item.Subtopic.Trim(),
                EstimatedHours = item.EstimatedHours,
                ActualHours = 0,
                TaskType = taskType,
                Notes = item.Notes.Trim(),
                Priority = priority,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        dbContext.StudyPlans.Add(plan);
        await dbContext.SaveChangesAsync(cancellationToken);
        return MapPlan(plan);
    }

    public async Task<IReadOnlyCollection<StudyPlanResponse>> GetPlansAsync(Guid userId)
    {
        var plans = await dbContext.StudyPlans
            .Where(x => x.UserId == userId)
            .Include(x => x.Tasks)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return plans.Select(MapPlan).ToList();
    }

    public async Task<StudyPlanResponse> GetPlanByIdAsync(Guid userId, Guid planId)
    {
        var plan = await dbContext.StudyPlans
            .Where(x => x.UserId == userId && x.Id == planId)
            .Include(x => x.Tasks)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Plan not found.");

        return MapPlan(plan);
    }

    public async Task<IReadOnlyCollection<StudyPlanResponse>> GetPlansByGoalAsync(Guid userId, Guid goalId)
    {
        var plans = await dbContext.StudyPlans
            .Where(x => x.UserId == userId && x.StudyGoalId == goalId)
            .Include(x => x.Tasks)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return plans.Select(MapPlan).ToList();
    }

    public async Task<IReadOnlyCollection<StudyTaskResponse>> GetTodayTasksAsync(Guid userId)
    {
        var today = DateTime.UtcNow.Date;
        var tasks = await dbContext.StudyTasks
            .Where(x => x.UserId == userId && x.TaskDate == today)
            .Include(x => x.StudyGoal)
            .OrderBy(x => x.TaskDate)
            .ToListAsync();
        return tasks.Select(MapTask).ToList();
    }

    public async Task<IReadOnlyCollection<StudyTaskResponse>> GetWeekTasksAsync(Guid userId)
    {
        var today = DateTime.UtcNow.Date;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(6);
        var tasks = await dbContext.StudyTasks
            .Where(x => x.UserId == userId && x.TaskDate >= startOfWeek && x.TaskDate <= endOfWeek)
            .Include(x => x.StudyGoal)
            .OrderBy(x => x.TaskDate)
            .ToListAsync();
        return tasks.Select(MapTask).ToList();
    }

    public async Task<StudyTaskResponse> UpdateTaskAsync(Guid userId, Guid taskId, StudyTaskUpdateRequest request)
    {
        var task = await dbContext.StudyTasks
            .Include(x => x.StudyGoal)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == taskId)
            ?? throw new KeyNotFoundException("Task not found.");

        task.TaskDate = request.TaskDate.Date;
        task.Topic = request.Topic.Trim();
        task.Subtopic = request.Subtopic.Trim();
        task.EstimatedHours = request.EstimatedHours;
        task.ActualHours = request.ActualHours;
        task.TaskType = request.TaskType;
        task.Notes = request.Notes.Trim();
        task.Priority = request.Priority;
        task.IsCompleted = request.IsCompleted;
        task.CompletedAt = request.IsCompleted ? (task.CompletedAt ?? DateTime.UtcNow) : null;
        task.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return MapTask(task);
    }

    public async Task<StudyTaskResponse> ToggleTaskCompletionAsync(Guid userId, Guid taskId)
    {
        var task = await dbContext.StudyTasks
            .Include(x => x.StudyGoal)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == taskId)
            ?? throw new KeyNotFoundException("Task not found.");

        task.IsCompleted = !task.IsCompleted;
        task.CompletedAt = task.IsCompleted ? DateTime.UtcNow : null;
        task.ActualHours = task.IsCompleted && task.ActualHours <= 0 ? task.EstimatedHours : task.ActualHours;
        task.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return MapTask(task);
    }

    public async Task DeleteTaskAsync(Guid userId, Guid taskId)
    {
        var task = await dbContext.StudyTasks.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == taskId)
            ?? throw new KeyNotFoundException("Task not found.");

        dbContext.StudyTasks.Remove(task);
        await dbContext.SaveChangesAsync();
    }

    private static StudyPlanResponse MapPlan(StudyPlan plan) => new()
    {
        Id = plan.Id,
        StudyGoalId = plan.StudyGoalId,
        Title = plan.Title,
        StartDate = plan.StartDate,
        EndDate = plan.EndDate,
        TotalEstimatedHours = plan.TotalEstimatedHours,
        GeneratedByAI = plan.GeneratedByAI,
        CreatedAt = plan.CreatedAt,
        Tasks = plan.Tasks.OrderBy(x => x.TaskDate).Select(MapTask).ToList()
    };

    private static StudyTaskResponse MapTask(StudyTask task) => new()
    {
        Id = task.Id,
        StudyPlanId = task.StudyPlanId,
        StudyGoalId = task.StudyGoalId,
        TaskDate = task.TaskDate,
        Topic = task.Topic,
        Subtopic = task.Subtopic,
        EstimatedHours = task.EstimatedHours,
        ActualHours = task.ActualHours,
        TaskType = task.TaskType,
        Notes = task.Notes,
        Priority = task.Priority,
        IsCompleted = task.IsCompleted,
        CompletedAt = task.CompletedAt,
        GoalTitle = task.StudyGoal?.Title ?? string.Empty
    };

    private async Task<string> BuildPlanningContextAsync(Guid userId, Guid goalId, CancellationToken cancellationToken)
    {
        var sevenDaysAgo = DateTime.UtcNow.Date.AddDays(-7);

        var recentHours = await dbContext.ProgressLogs
            .Where(x => x.UserId == userId && x.Date >= sevenDaysAgo)
            .SumAsync(x => (decimal?)x.HoursSpent, cancellationToken) ?? 0;

        var totalTasksForGoal = await dbContext.StudyTasks
            .CountAsync(x => x.UserId == userId && x.StudyGoalId == goalId, cancellationToken);

        var completedTasksForGoal = await dbContext.StudyTasks
            .CountAsync(x => x.UserId == userId && x.StudyGoalId == goalId && x.IsCompleted, cancellationToken);

        var pendingReminders = await dbContext.Reminders
            .CountAsync(x => x.UserId == userId && !x.IsSent, cancellationToken);

        return $"recentHoursLast7Days={recentHours};goalTasksCompleted={completedTasksForGoal};goalTasksTotal={totalTasksForGoal};pendingReminders={pendingReminders}";
    }

    private static string InferProvider(AiRequestLog requestLog)
    {
        if (requestLog.Status == AiRequestStatus.Fallback)
        {
            return "Fallback";
        }

        if (requestLog.Status == AiRequestStatus.Failed)
        {
            return "Unknown";
        }

        if (string.IsNullOrWhiteSpace(requestLog.Response))
        {
            return "Unknown";
        }

        try
        {
            using var document = JsonDocument.Parse(requestLog.Response);
            var root = document.RootElement;

            if (root.TryGetProperty("candidates", out _))
            {
                return "Gemini";
            }

            if (root.TryGetProperty("choices", out _))
            {
                return "Groq";
            }

            if (root.TryGetProperty("provider", out var providerElement) &&
                providerElement.ValueKind == JsonValueKind.String)
            {
                return providerElement.GetString() ?? "Unknown";
            }
        }
        catch (JsonException)
        {
            // Ignore parse issues and use a string heuristic below.
        }

        if (requestLog.Response.Contains("\"candidates\"", StringComparison.OrdinalIgnoreCase))
        {
            return "Gemini";
        }

        if (requestLog.Response.Contains("\"choices\"", StringComparison.OrdinalIgnoreCase))
        {
            return "Groq";
        }

        return "Unknown";
    }

    private static string BuildProviderStatusMessage(AiRequestLog requestLog, string provider)
    {
        if (requestLog.Status == AiRequestStatus.Success)
        {
            return $"Last AI plan generation succeeded via {provider}.";
        }

        if (requestLog.Status == AiRequestStatus.Failed)
        {
            if (!string.IsNullOrWhiteSpace(requestLog.ErrorMessage))
            {
                return $"Last AI plan generation failed: {Truncate(requestLog.ErrorMessage, 240)}";
            }

            return "Last AI plan generation failed.";
        }

        var fallbackDetails = ExtractFallbackDetails(requestLog.Response);
        if (string.IsNullOrWhiteSpace(fallbackDetails))
        {
            return "Used deterministic fallback because AI providers were unavailable.";
        }

        return $"Used deterministic fallback. {fallbackDetails}";
    }

    private static string ExtractFallbackDetails(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(response);
            var root = document.RootElement;

            string? geminiError = null;
            string? groqError = null;

            if (root.TryGetProperty("geminiError", out var geminiElement) && geminiElement.ValueKind == JsonValueKind.String)
            {
                geminiError = geminiElement.GetString();
            }

            if (root.TryGetProperty("groqError", out var groqElement) && groqElement.ValueKind == JsonValueKind.String)
            {
                groqError = groqElement.GetString();
            }

            if (!string.IsNullOrWhiteSpace(geminiError) && !string.IsNullOrWhiteSpace(groqError))
            {
                return $"Gemini: {Truncate(geminiError, 120)} Groq: {Truncate(groqError, 120)}";
            }

            if (!string.IsNullOrWhiteSpace(geminiError))
            {
                return $"Gemini: {Truncate(geminiError, 180)}";
            }

            if (!string.IsNullOrWhiteSpace(groqError))
            {
                return $"Groq: {Truncate(groqError, 180)}";
            }
        }
        catch (JsonException)
        {
            // Ignore parse issues and fall back to a generic message.
        }

        return string.Empty;
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }
}
