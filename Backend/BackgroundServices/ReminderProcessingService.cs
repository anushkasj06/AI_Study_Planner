using AIStudyPlanner.Api.Interfaces;

namespace AIStudyPlanner.Api.BackgroundServices;

public class ReminderProcessingService(
    IServiceScopeFactory scopeFactory,
    ILogger<ReminderProcessingService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService>();
                var processed = await reminderService.ProcessDueRemindersAsync(stoppingToken);
                if (processed > 0)
                {
                    logger.LogInformation("Processed {Count} reminders.", processed);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Reminder processing loop failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
