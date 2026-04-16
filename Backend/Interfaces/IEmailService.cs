namespace AIStudyPlanner.Api.Interfaces;

public interface IEmailService
{
    Task SendReminderAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
}
