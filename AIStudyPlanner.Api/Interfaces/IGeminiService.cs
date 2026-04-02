using AIStudyPlanner.Api.DTOs.Plans;
using AIStudyPlanner.Api.Entities;

namespace AIStudyPlanner.Api.Interfaces;

public interface IGeminiService
{
    Task<GeneratedPlanResult> GenerateStudyPlanAsync(User user, StudyGoal goal, CancellationToken cancellationToken = default);
}
