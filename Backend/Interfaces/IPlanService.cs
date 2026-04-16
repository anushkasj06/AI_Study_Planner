using AIStudyPlanner.Api.DTOs.Plans;
using AIStudyPlanner.Api.DTOs.Tasks;

namespace AIStudyPlanner.Api.Interfaces;

public interface IPlanService
{
    Task<StudyPlanResponse> GeneratePlanAsync(Guid userId, Guid goalId, bool forceRegenerate, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<StudyPlanResponse>> GetPlansAsync(Guid userId);
    Task<StudyPlanResponse> GetPlanByIdAsync(Guid userId, Guid planId);
    Task<IReadOnlyCollection<StudyPlanResponse>> GetPlansByGoalAsync(Guid userId, Guid goalId);
    Task<IReadOnlyCollection<StudyTaskResponse>> GetTodayTasksAsync(Guid userId);
    Task<IReadOnlyCollection<StudyTaskResponse>> GetWeekTasksAsync(Guid userId);
    Task<StudyTaskResponse> UpdateTaskAsync(Guid userId, Guid taskId, StudyTaskUpdateRequest request);
    Task<StudyTaskResponse> ToggleTaskCompletionAsync(Guid userId, Guid taskId);
    Task DeleteTaskAsync(Guid userId, Guid taskId);
}
