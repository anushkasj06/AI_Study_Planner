using System.Text.Json;
using AIStudyPlanner.Api.Data;
using AIStudyPlanner.Api.Helpers;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebPush;

namespace AIStudyPlanner.Api.Services;

public class WebPushService(
    ApplicationDbContext dbContext,
    IOptions<WebPushOptions> options,
    ILogger<WebPushService> logger) : IWebPushService
{
    private readonly WebPushOptions _options = options.Value;

    public Task<string> GetPublicKeyAsync()
    {
        return Task.FromResult(_options.PublicKey);
    }

    public async Task SaveSubscriptionAsync(
        Guid userId,
        string endpoint,
        string p256dh,
        string auth,
        CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.WebPushSubscriptions
            .FirstOrDefaultAsync(x => x.Endpoint == endpoint, cancellationToken);

        if (existing is null)
        {
            dbContext.WebPushSubscriptions.Add(new Entities.WebPushSubscription
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Endpoint = endpoint.Trim(),
                P256dh = p256dh.Trim(),
                Auth = auth.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.UserId = userId;
            existing.P256dh = p256dh.Trim();
            existing.Auth = auth.Trim();
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveSubscriptionAsync(Guid userId, string endpoint, CancellationToken cancellationToken = default)
    {
        var subscription = await dbContext.WebPushSubscriptions
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Endpoint == endpoint, cancellationToken);

        if (subscription is null)
        {
            return;
        }

        dbContext.WebPushSubscriptions.Remove(subscription);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<DeliveryResult> SendToUserAsync(Guid userId, string title, string message, CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            logger.LogWarning("Web push keys are not configured. Push message skipped for user {UserId}.", userId);
            return DeliveryResult.Fail("Web push is not configured.");
        }

        var subscriptions = await dbContext.WebPushSubscriptions
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        if (subscriptions.Count == 0)
        {
            logger.LogWarning("No web push subscriptions found for user {UserId}.", userId);
            return DeliveryResult.Fail("No web push subscription found for this user.", 0, 0);
        }

        var vapidDetails = new VapidDetails(_options.Subject, _options.PublicKey, _options.PrivateKey);
        var client = new WebPushClient();
        var deliveredCount = 0;

        var payload = JsonSerializer.Serialize(new
        {
            title,
            body = message,
            icon = "/icon-192.png",
            badge = "/icon-192.png"
        });

        foreach (var subscription in subscriptions)
        {
            var pushSubscription = new PushSubscription(subscription.Endpoint, subscription.P256dh, subscription.Auth);
            try
            {
                await client.SendNotificationAsync(pushSubscription, payload, vapidDetails, cancellationToken: cancellationToken);
                deliveredCount += 1;
            }
            catch (WebPushException ex)
            {
                logger.LogWarning(ex, "Push notification failed for endpoint {Endpoint}. Removing stale subscription.", subscription.Endpoint);
                dbContext.WebPushSubscriptions.Remove(subscription);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        if (deliveredCount == 0)
        {
            return DeliveryResult.Fail("Web push delivery failed for all subscriptions.", subscriptions.Count, 0);
        }

        if (deliveredCount < subscriptions.Count)
        {
            return DeliveryResult.Ok(
                $"Web push sent to {deliveredCount} device(s); {subscriptions.Count - deliveredCount} subscription(s) failed.",
                subscriptions.Count,
                deliveredCount);
        }

        return DeliveryResult.Ok($"Web push sent to {deliveredCount} device(s).", subscriptions.Count, deliveredCount);
    }
}
