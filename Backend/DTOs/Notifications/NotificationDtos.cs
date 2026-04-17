using AIStudyPlanner.Api.Entities;
using FluentValidation;

namespace AIStudyPlanner.Api.DTOs.Notifications;

public class NotificationResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WebPushSubscriptionRequest
{
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}

public class WebPushPublicKeyResponse
{
    public string PublicKey { get; set; } = string.Empty;
}

public class WebPushSubscriptionRequestValidator : AbstractValidator<WebPushSubscriptionRequest>
{
    public WebPushSubscriptionRequestValidator()
    {
        RuleFor(x => x.Endpoint).NotEmpty().MaximumLength(700);
        RuleFor(x => x.P256dh).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Auth).NotEmpty().MaximumLength(300);
    }
}
