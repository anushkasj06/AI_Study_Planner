using AIStudyPlanner.Api.Data;
using AIStudyPlanner.Api.DTOs.Progress;
using AIStudyPlanner.Api.Entities;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AIStudyPlanner.Api.Services;

public class ProgressService(ApplicationDbContext dbContext) : IProgressService
{
    public async Task<ProgressLogResponse> LogProgressAsync(Guid userId, ProgressLogCreateRequest request)
    {
        var task = await dbContext.StudyTasks.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == request.StudyTaskId)
            ?? throw new KeyNotFoundException("Task not found.");

        if (!await dbContext.StudyGoals.AnyAsync(x => x.UserId == userId && x.Id == request.StudyGoalId))
        {
            throw new KeyNotFoundException("Goal not found.");
        }

        var progress = new ProgressLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StudyTaskId = request.StudyTaskId,
            StudyGoalId = request.StudyGoalId,
            Date = request.Date.Date,
            HoursSpent = request.HoursSpent,
            Notes = request.Notes.Trim(),
            CompletionPercentage = request.CompletionPercentage,
            CreatedAt = DateTime.UtcNow
        };

        task.ActualHours = Math.Max(task.ActualHours, request.HoursSpent);
        task.IsCompleted = request.CompletionPercentage >= 100 || task.IsCompleted;
        task.CompletedAt = task.IsCompleted ? (task.CompletedAt ?? DateTime.UtcNow) : null;
        task.UpdatedAt = DateTime.UtcNow;

        dbContext.ProgressLogs.Add(progress);
        await dbContext.SaveChangesAsync();

        return new ProgressLogResponse
        {
            Id = progress.Id,
            StudyTaskId = progress.StudyTaskId,
            StudyGoalId = progress.StudyGoalId,
            Date = progress.Date,
            HoursSpent = progress.HoursSpent,
            Notes = progress.Notes,
            CompletionPercentage = progress.CompletionPercentage,
            CreatedAt = progress.CreatedAt
        };
    }

    public async Task<ProgressSummaryResponse> GetSummaryAsync(Guid userId)
    {
        var weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        var weekEnd = weekStart.AddDays(6);

        var completedHours = await dbContext.ProgressLogs
            .Where(x => x.UserId == userId && x.Date >= weekStart && x.Date <= weekEnd)
            .SumAsync(x => (decimal?)x.HoursSpent) ?? 0;

        var totalTasks = await dbContext.StudyTasks.CountAsync(x => x.UserId == userId);
        var completedTasks = await dbContext.StudyTasks.CountAsync(x => x.UserId == userId && x.IsCompleted);

        return new ProgressSummaryResponse
        {
            CompletedHoursThisWeek = completedHours,
            CompletedTasks = completedTasks,
            TotalTasks = totalTasks,
            DailyStreak = await CalculateStreakAsync(userId),
            WeeklyProgressPercentage = totalTasks == 0 ? 0 : decimal.Round((decimal)completedTasks / totalTasks * 100m, 2)
        };
    }

    public async Task<GoalProgressResponse> GetGoalProgressAsync(Guid userId, Guid goalId)
    {
        var goal = await dbContext.StudyGoals.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == goalId)
            ?? throw new KeyNotFoundException("Goal not found.");

        var totalTasks = await dbContext.StudyTasks.CountAsync(x => x.UserId == userId && x.StudyGoalId == goalId);
        var completedTasks = await dbContext.StudyTasks.CountAsync(x => x.UserId == userId && x.StudyGoalId == goalId && x.IsCompleted);
        var hoursSpent = await dbContext.ProgressLogs
            .Where(x => x.UserId == userId && x.StudyGoalId == goalId)
            .SumAsync(x => (decimal?)x.HoursSpent) ?? 0;

        return new GoalProgressResponse
        {
            GoalId = goal.Id,
            GoalTitle = goal.Title,
            GoalCompletionPercentage = totalTasks == 0 ? 0 : decimal.Round((decimal)completedTasks / totalTasks * 100m, 2),
            HoursSpent = hoursSpent,
            CompletedTasks = completedTasks,
            TotalTasks = totalTasks
        };
    }

    public async Task<StreakResponse> GetStreakAsync(Guid userId)
    {
        return new StreakResponse { CurrentStreak = await CalculateStreakAsync(userId) };
    }

    private async Task<int> CalculateStreakAsync(Guid userId)
    {
        var days = await dbContext.ProgressLogs
            .Where(x => x.UserId == userId)
            .Select(x => x.Date.Date)
            .Distinct()
            .OrderByDescending(x => x)
            .ToListAsync();

        var streak = 0;
        var cursor = DateTime.UtcNow.Date;

        foreach (var day in days)
        {
            if (day == cursor)
            {
                streak++;
                cursor = cursor.AddDays(-1);
            }
            else if (day < cursor)
            {
                break;
            }
        }

        return streak;
    }
}
