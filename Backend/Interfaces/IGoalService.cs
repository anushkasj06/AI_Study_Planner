using AIStudyPlanner.Api.DTOs.Goals;

namespace AIStudyPlanner.Api.Interfaces;

public interface IGoalService
{
    Task<IReadOnlyCollection<StudyGoalResponse>> GetGoalsAsync(Guid userId);
    Task<StudyGoalResponse> GetGoalByIdAsync(Guid userId, Guid goalId);
    Task<StudyGoalResponse> CreateGoalAsync(Guid userId, StudyGoalCreateRequest request);
    Task<StudyGoalResponse> UpdateGoalAsync(Guid userId, Guid goalId, StudyGoalUpdateRequest request);
    Task DeleteGoalAsync(Guid userId, Guid goalId);
}
