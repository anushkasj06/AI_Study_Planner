using System.Text.Json;
using AIStudyPlanner.Api.Data;
using AIStudyPlanner.Api.DTOs.Goals;
using AIStudyPlanner.Api.Entities;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AIStudyPlanner.Api.Services;

public class GoalService(ApplicationDbContext dbContext) : IGoalService
{
    public async Task<IReadOnlyCollection<StudyGoalResponse>> GetGoalsAsync(Guid userId)
    {
        var goals = await dbContext.StudyGoals
            .Where(x => x.UserId == userId)
            .Include(x => x.StudyTasks)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return goals.Select(Map).ToList();
    }

    public async Task<StudyGoalResponse> GetGoalByIdAsync(Guid userId, Guid goalId)
    {
        var goal = await dbContext.StudyGoals
            .Include(x => x.StudyTasks)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == goalId)
            ?? throw new KeyNotFoundException("Goal not found.");

        return Map(goal);
    }

    public async Task<StudyGoalResponse> CreateGoalAsync(Guid userId, StudyGoalCreateRequest request)
    {
        var goal = new StudyGoal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            TargetDate = request.TargetDate.Date,
            DailyAvailableHours = request.DailyAvailableHours,
            DifficultyLevel = request.DifficultyLevel,
            Priority = request.Priority,
            PreferredStudyTime = request.PreferredStudyTime,
            BreakPreference = request.BreakPreference,
            SubjectsJson = JsonSerializer.Serialize(request.Subjects),
            Status = request.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.StudyGoals.Add(goal);
        await dbContext.SaveChangesAsync();
        return Map(goal);
    }

    public async Task<StudyGoalResponse> UpdateGoalAsync(Guid userId, Guid goalId, StudyGoalUpdateRequest request)
    {
        var goal = await dbContext.StudyGoals
            .Include(x => x.StudyTasks)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Id == goalId)
            ?? throw new KeyNotFoundException("Goal not found.");

        goal.Title = request.Title.Trim();
        goal.Description = request.Description.Trim();
        goal.TargetDate = request.TargetDate.Date;
        goal.DailyAvailableHours = request.DailyAvailableHours;
        goal.DifficultyLevel = request.DifficultyLevel;
        goal.Priority = request.Priority;
        goal.PreferredStudyTime = request.PreferredStudyTime;
        goal.BreakPreference = request.BreakPreference;
        goal.SubjectsJson = JsonSerializer.Serialize(request.Subjects);
        goal.Status = request.Status;
        goal.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();
        return Map(goal);
    }

    public async Task DeleteGoalAsync(Guid userId, Guid goalId)
    {
        var goal = await dbContext.StudyGoals.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == goalId)
            ?? throw new KeyNotFoundException("Goal not found.");

        dbContext.StudyGoals.Remove(goal);
        await dbContext.SaveChangesAsync();
    }

    private static StudyGoalResponse Map(StudyGoal goal)
    {
        var totalTasks = goal.StudyTasks.Count;
        var completedTasks = goal.StudyTasks.Count(x => x.IsCompleted);

        return new StudyGoalResponse
        {
            Id = goal.Id,
            Title = goal.Title,
            Description = goal.Description,
            TargetDate = goal.TargetDate,
            DailyAvailableHours = goal.DailyAvailableHours,
            DifficultyLevel = goal.DifficultyLevel,
            Priority = goal.Priority,
            PreferredStudyTime = goal.PreferredStudyTime,
            BreakPreference = goal.BreakPreference,
            Subjects = JsonSerializer.Deserialize<List<string>>(goal.SubjectsJson) ?? [],
            Status = goal.Status,
            TotalTasks = totalTasks,
            CompletedTasks = completedTasks,
            CompletionPercentage = totalTasks == 0 ? 0 : decimal.Round((decimal)completedTasks / totalTasks * 100m, 2),
            CreatedAt = goal.CreatedAt,
            UpdatedAt = goal.UpdatedAt
        };
    }
}
