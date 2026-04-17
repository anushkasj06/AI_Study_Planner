using AIStudyPlanner.Api.DTOs.Notifications;
using AIStudyPlanner.Api.Entities;

namespace AIStudyPlanner.Api.Interfaces;

public interface INotificationService
{
    Task<IReadOnlyCollection<NotificationResponse>> GetNotificationsAsync(Guid userId, bool includeRead);
    Task<NotificationResponse> MarkReadAsync(Guid userId, Guid notificationId);
    Task<int> MarkAllReadAsync(Guid userId);
    Task<UserNotification> CreateAsync(Guid userId, string title, string message, NotificationType type, Guid? reminderId = null, CancellationToken cancellationToken = default);
}
