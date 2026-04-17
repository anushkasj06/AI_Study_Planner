using AIStudyPlanner.Api.DTOs.Assistant;

namespace AIStudyPlanner.Api.Interfaces;

public interface IAssistantComposerService
{
    Task<AssistantChatResponse> CreateChatResponseAsync(Guid userId, string message, CancellationToken cancellationToken = default);

    Task<AssistantNoteDraftResponse> CreateNoteDraftAsync(Guid userId, Guid? studyGoalId, string prompt, CancellationToken cancellationToken = default);
}
