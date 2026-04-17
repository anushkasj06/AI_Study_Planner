using AIStudyPlanner.Api.DTOs.Assistant;

namespace AIStudyPlanner.Api.Interfaces;

public interface IStudyAssistantService
{
    Task<AssistantChatResponse> ChatAsync(Guid userId, AssistantChatRequest request, CancellationToken cancellationToken = default);
}
