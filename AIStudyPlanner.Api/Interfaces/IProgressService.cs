using AIStudyPlanner.Api.DTOs.Progress;

namespace AIStudyPlanner.Api.Interfaces;

public interface IProgressService
{
    Task<ProgressLogResponse> LogProgressAsync(Guid userId, ProgressLogCreateRequest request);
    Task<ProgressSummaryResponse> GetSummaryAsync(Guid userId);
    Task<GoalProgressResponse> GetGoalProgressAsync(Guid userId, Guid goalId);
    Task<StreakResponse> GetStreakAsync(Guid userId);
}
