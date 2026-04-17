using AIStudyPlanner.Api.DTOs.Notifications;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NotificationsController(
    INotificationService notificationService,
    IWebPushService webPushService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<NotificationResponse>>> Get([FromQuery] bool includeRead = true)
    {
        return Ok(await notificationService.GetNotificationsAsync(currentUserService.GetUserId(), includeRead));
    }

    [HttpPatch("{id:guid}/mark-read")]
    public async Task<ActionResult<NotificationResponse>> MarkRead(Guid id)
    {
        return Ok(await notificationService.MarkReadAsync(currentUserService.GetUserId(), id));
    }

    [HttpPatch("mark-all-read")]
    public async Task<ActionResult<object>> MarkAllRead()
    {
        var count = await notificationService.MarkAllReadAsync(currentUserService.GetUserId());
        return Ok(new { updated = count });
    }

    [HttpGet("webpush/public-key")]
    public async Task<ActionResult<WebPushPublicKeyResponse>> GetWebPushPublicKey()
    {
        return Ok(new WebPushPublicKeyResponse
        {
            PublicKey = await webPushService.GetPublicKeyAsync()
        });
    }

    [HttpPost("webpush/subscribe")]
    public async Task<IActionResult> Subscribe(WebPushSubscriptionRequest request)
    {
        await webPushService.SaveSubscriptionAsync(
            currentUserService.GetUserId(),
            request.Endpoint,
            request.P256dh,
            request.Auth);

        return NoContent();
    }

    [HttpPost("webpush/unsubscribe")]
    public async Task<IActionResult> Unsubscribe([FromBody] WebPushSubscriptionRequest request)
    {
        await webPushService.RemoveSubscriptionAsync(currentUserService.GetUserId(), request.Endpoint);
        return NoContent();
    }
}
