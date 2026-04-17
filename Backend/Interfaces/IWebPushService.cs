using AIStudyPlanner.Api.Helpers;

namespace AIStudyPlanner.Api.Interfaces;

public interface IWebPushService
{
    Task<string> GetPublicKeyAsync();
    Task SaveSubscriptionAsync(Guid userId, string endpoint, string p256dh, string auth, CancellationToken cancellationToken = default);
    Task RemoveSubscriptionAsync(Guid userId, string endpoint, CancellationToken cancellationToken = default);
    Task<DeliveryResult> SendToUserAsync(Guid userId, string title, string message, CancellationToken cancellationToken = default);
}
