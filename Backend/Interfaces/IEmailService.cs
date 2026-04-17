using AIStudyPlanner.Api.Helpers;

namespace AIStudyPlanner.Api.Interfaces;

public interface IEmailService
{
    Task<DeliveryResult> SendReminderAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
}
