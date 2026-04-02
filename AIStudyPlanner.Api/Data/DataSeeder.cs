using System.Text.Json;
using AIStudyPlanner.Api.Entities;
using AIStudyPlanner.Api.Helpers;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AIStudyPlanner.Api.Data;

public class DataSeeder(ApplicationDbContext dbContext) : IDataSeeder
{
    public async Task SeedAsync()
    {
        if (await dbContext.Users.AnyAsync())
        {
            return;
        }

        var userId = Guid.NewGuid();
        var goalId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var task1Id = Guid.NewGuid();
        var task2Id = Guid.NewGuid();

        dbContext.Users.Add(new User
        {
            Id = userId,
            FullName = "Demo Student",
            Email = AppConstants.DemoUserEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(AppConstants.DemoUserPassword),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        dbContext.StudyGoals.Add(new StudyGoal
        {
            Id = goalId,
            UserId = userId,
            Title = "Complete DSA in 45 days",
            Description = "Finish arrays, linked lists, trees, graphs, and revision rounds.",
            TargetDate = DateTime.UtcNow.Date.AddDays(45),
            DailyAvailableHours = 2.5m,
            DifficultyLevel = DifficultyLevel.Intermediate,
            Priority = GoalPriority.High,
            PreferredStudyTime = PreferredStudyTime.Evening,
            BreakPreference = BreakPreference.Pomodoro,
            SubjectsJson = JsonSerializer.Serialize(new[] { "Arrays", "Trees", "Graphs", "Revision" }),
            Status = GoalStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        dbContext.StudyPlans.Add(new StudyPlan
        {
            Id = planId,
            UserId = userId,
            StudyGoalId = goalId,
            Title = "DSA Interview Ramp-Up",
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddDays(7),
            TotalEstimatedHours = 6.5m,
            GeneratedByAI = false,
            RawAiPrompt = "Seed data plan",
            RawAiResponse = "{}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        dbContext.StudyTasks.AddRange(
            new StudyTask
            {
                Id = task1Id,
                StudyPlanId = planId,
                StudyGoalId = goalId,
                UserId = userId,
                TaskDate = DateTime.UtcNow.Date,
                Topic = "Arrays",
                Subtopic = "Sliding window",
                EstimatedHours = 2,
                ActualHours = 1.5m,
                TaskType = TaskType.Learn,
                Notes = "Practice two easy and one medium problem.",
                Priority = GoalPriority.High,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new StudyTask
            {
                Id = task2Id,
                StudyPlanId = planId,
                StudyGoalId = goalId,
                UserId = userId,
                TaskDate = DateTime.UtcNow.Date.AddDays(1),
                Topic = "Trees",
                Subtopic = "Binary tree traversals",
                EstimatedHours = 2.5m,
                ActualHours = 0,
                TaskType = TaskType.Practice,
                Notes = "Implement DFS and BFS variants.",
                Priority = GoalPriority.High,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

        dbContext.Reminders.Add(new Reminder
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StudyTaskId = task1Id,
            Title = "Study Arrays at 7 PM",
            Message = "Keep the first session light and focus on patterns.",
            ReminderDateTime = DateTime.UtcNow.AddHours(4),
            IsSent = false,
            IsRead = false,
            Channel = ReminderChannel.InApp,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        dbContext.ProgressLogs.Add(new ProgressLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StudyTaskId = task1Id,
            StudyGoalId = goalId,
            Date = DateTime.UtcNow.Date,
            HoursSpent = 1.5m,
            Notes = "Completed the first guided session.",
            CompletionPercentage = 60,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }
}
