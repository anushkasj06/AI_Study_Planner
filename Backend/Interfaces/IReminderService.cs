using AIStudyPlanner.Api.DTOs.Reminders;

namespace AIStudyPlanner.Api.Interfaces;

public interface IReminderService
{
    Task<IReadOnlyCollection<ReminderResponse>> GetRemindersAsync(Guid userId);
    Task<ReminderResponse> CreateReminderAsync(Guid userId, ReminderCreateRequest request);
    Task<IReadOnlyCollection<ReminderResponse>> CreateReminderBatchAsync(Guid userId, ReminderBatchCreateRequest request, CancellationToken cancellationToken = default);
    Task<AiReminderDraftResponse> GenerateAiReminderDraftAsync(Guid userId, AiReminderDraftRequest request, CancellationToken cancellationToken = default);
    Task<int> GenerateSmartRemindersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ReminderResponse> UpdateReminderAsync(Guid userId, Guid reminderId, ReminderUpdateRequest request);
    Task DeleteReminderAsync(Guid userId, Guid reminderId);
    Task<ReminderResponse> MarkReadAsync(Guid userId, Guid reminderId);
    Task<int> ProcessDueRemindersAsync(CancellationToken cancellationToken = default);
    Task<int> ProcessDueRemindersForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
