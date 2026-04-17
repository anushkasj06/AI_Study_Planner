using AIStudyPlanner.Api.Data;
using AIStudyPlanner.Api.DTOs.Dashboard;
using AIStudyPlanner.Api.Entities;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AIStudyPlanner.Api.Services;

public class DashboardService(ApplicationDbContext dbContext, IProgressService progressService) : IDashboardService
{
    public async Task<DashboardSummaryResponse> GetSummaryAsync(Guid userId)
    {
        var totalGoals = await dbContext.StudyGoals.CountAsync(x => x.UserId == userId);
        var activeGoals = await dbContext.StudyGoals.CountAsync(x => x.UserId == userId && x.Status == GoalStatus.Active);
        var completedTasks = await dbContext.StudyTasks.CountAsync(x => x.UserId == userId && x.IsCompleted);
        var pendingTasks = await dbContext.StudyTasks.CountAsync(x => x.UserId == userId && !x.IsCompleted);
        var unreadNotifications = await dbContext.UserNotifications.CountAsync(x => x.UserId == userId && !x.IsRead);
        var progressSummary = await progressService.GetSummaryAsync(userId);

        var reminders = await dbContext.Reminders
            .Where(x => x.UserId == userId && x.ReminderDateTime >= DateTime.UtcNow)
            .OrderBy(x => x.ReminderDateTime)
            .Take(5)
            .Select(x => new UpcomingReminderItem
            {
                Id = x.Id,
                Title = x.Title,
                ReminderDateTime = x.ReminderDateTime,
                IsRead = x.IsRead
            })
            .ToListAsync();

        var goalProgress = await dbContext.StudyGoals
            .Where(x => x.UserId == userId)
            .Select(goal => new GoalProgressWidget
            {
                GoalId = goal.Id,
                GoalTitle = goal.Title,
                CompletionPercentage = goal.StudyTasks.Count == 0
                    ? 0
                    : decimal.Round((decimal)goal.StudyTasks.Count(t => t.IsCompleted) / goal.StudyTasks.Count * 100m, 2)
            })
            .ToListAsync();

        return new DashboardSummaryResponse
        {
            TotalGoals = totalGoals,
            ActiveGoals = activeGoals,
            CompletedTasks = completedTasks,
            PendingTasks = pendingTasks,
            HoursStudiedThisWeek = progressSummary.CompletedHoursThisWeek,
            StreakCount = progressSummary.DailyStreak,
            UnreadNotifications = unreadNotifications,
            UpcomingReminders = reminders,
            ProgressByGoal = goalProgress
        };
    }
}
