using System.Net;
using System.Net.Mail;
using AIStudyPlanner.Api.Helpers;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.Extensions.Options;

namespace AIStudyPlanner.Api.Services;

public class EmailService(IOptions<SmtpOptions> smtpOptions, ILogger<EmailService> logger) : IEmailService
{
    private readonly SmtpOptions _smtpOptions = smtpOptions.Value;

    public async Task<DeliveryResult> SendReminderAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (!_smtpOptions.IsConfigured)
        {
            logger.LogWarning("SMTP not configured. Email reminder delivery skipped for {Email}.", toEmail);
            return DeliveryResult.Fail("SMTP is not configured.");
        }

        try
        {
            using var message = new MailMessage(_smtpOptions.FromEmail, toEmail, subject, body);
            using var client = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpOptions.Username, _smtpOptions.Password)
            };

            cancellationToken.ThrowIfCancellationRequested();
            await client.SendMailAsync(message, cancellationToken);
            logger.LogInformation("Email reminder sent to {Email}.", toEmail);
            return DeliveryResult.Ok("Email sent.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Email reminder delivery failed for {Email}.", toEmail);
            return DeliveryResult.Fail($"Email delivery failed: {ex.Message}");
        }
    }
}
