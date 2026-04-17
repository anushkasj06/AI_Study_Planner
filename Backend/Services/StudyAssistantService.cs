using AIStudyPlanner.Api.DTOs.Assistant;
using AIStudyPlanner.Api.Interfaces;

namespace AIStudyPlanner.Api.Services;

public class StudyAssistantService(IAssistantComposerService assistantComposerService) : IStudyAssistantService
{
    public async Task<AssistantChatResponse> ChatAsync(Guid userId, AssistantChatRequest request, CancellationToken cancellationToken = default)
    {
        return await assistantComposerService.CreateChatResponseAsync(userId, request.Message, cancellationToken);
    }
}
