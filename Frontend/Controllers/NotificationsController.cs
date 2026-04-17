using AIStudyPlanner.Web.Models;
using AIStudyPlanner.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Web.Controllers;

[Authorize]
public sealed class NotificationsController : Controller
{
    private readonly StudyPlannerApiClient _apiClient;

    public NotificationsController(StudyPlannerApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet("/app/notifications")]
    public async Task<IActionResult> Get([FromQuery] bool includeRead = true)
    {
        var notifications = await _apiClient.NotificationsAsync(includeRead);
        return Json(notifications);
    }

    [HttpPost("/app/notifications/{notificationId:guid}/mark-read")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(Guid notificationId)
    {
        var updated = await _apiClient.MarkNotificationReadAsync(notificationId);
        return Json(updated);
    }

    [HttpPost("/app/notifications/mark-all-read")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        await _apiClient.MarkAllNotificationsReadAsync();
        return NoContent();
    }

    [HttpGet("/app/notifications/webpush/public-key")]
    public async Task<IActionResult> GetWebPushPublicKey()
    {
        var key = await _apiClient.GetWebPushPublicKeyAsync();
        return Json(key);
    }

    [HttpPost("/app/notifications/webpush/subscribe")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe([FromBody] WebPushSubscriptionRequest request)
    {
        await _apiClient.SubscribeWebPushAsync(request);
        return NoContent();
    }

    [HttpPost("/app/notifications/webpush/unsubscribe")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unsubscribe([FromBody] WebPushSubscriptionRequest request)
    {
        await _apiClient.UnsubscribeWebPushAsync(request);
        return NoContent();
    }
}
