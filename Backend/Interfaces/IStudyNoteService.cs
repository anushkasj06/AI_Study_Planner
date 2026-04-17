using AIStudyPlanner.Api.DTOs.Assistant;

namespace AIStudyPlanner.Api.Interfaces;

public interface IStudyNoteService
{
    Task<IReadOnlyCollection<StudyNoteResponse>> GetNotesAsync(Guid userId);
    Task<StudyNoteResponse> CreateFromAssistantAsync(Guid userId, CreateNoteFromAssistantRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid userId, Guid noteId, CancellationToken cancellationToken = default);
}
