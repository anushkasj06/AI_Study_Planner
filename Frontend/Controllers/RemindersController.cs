using AIStudyPlanner.Web.Models;
using AIStudyPlanner.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Web.Controllers;

[Authorize]
public sealed class RemindersController : Controller
{
    private readonly StudyPlannerApiClient _apiClient;
    private readonly ILogger<RemindersController> _logger;

    public RemindersController(StudyPlannerApiClient apiClient, ILogger<RemindersController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    [HttpGet("/app/reminders")]
    public async Task<IActionResult> Index()
    {
        var reminders = await _apiClient.RemindersAsync();
        return View(new RemindersPageViewModel
        {
            Reminders = reminders
        });
    }

    [HttpPost("/app/reminders/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Form")] ReminderFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return await ReturnIndexViewAsync(model);
        }

        if (!DateTime.TryParse(model.ReminderDateTime, out var reminderDateTime))
        {
            ModelState.AddModelError(nameof(model.ReminderDateTime), "Select a valid reminder time.");
            return await ReturnIndexViewAsync(model);
        }

        var channels = ExtractSelectedChannels(model);
        if (channels.Count == 0)
        {
            ModelState.AddModelError(nameof(model.ChannelInApp), "Choose at least one delivery channel.");
            return await ReturnIndexViewAsync(model);
        }

        var reminderUtc = NormalizeToUtc(reminderDateTime);

        try
        {
            var created = await _apiClient.CreateReminderBatchAsync(new ReminderBatchCreateRequest
            {
                Title = model.Title,
                Message = model.Message,
                ReminderDateTime = reminderUtc,
                Channels = channels
            });

            TempData["SuccessMessage"] = $"{created.Count} reminder(s) scheduled.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Reminder batch create failed.");
            ModelState.AddModelError(string.Empty, ex.Message);
            return await ReturnIndexViewAsync(model);
        }
    }

    [HttpPost("/app/reminders/ai-draft")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateAiDraft([FromBody] AiReminderDraftRequest request)
    {
        try
        {
            if (request.PreferredReminderDateTime.HasValue)
            {
                request.PreferredReminderDateTime = NormalizeToUtc(request.PreferredReminderDateTime.Value);
            }

            var draft = await _apiClient.GenerateAiReminderDraftAsync(request);
            return Json(draft);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI reminder draft generation failed.");
            Response.StatusCode = 400;
            return Json(new { message = ex.Message });
        }
    }

    [HttpPost("/app/reminders/ai-sync")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SyncAiReminders()
    {
        try
        {
            var response = await _apiClient.SyncAiRemindersAsync();
            TempData["SuccessMessage"] = response.Message;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI reminder sync failed.");
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/app/reminders/process-now")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessNow()
    {
        try
        {
            var response = await _apiClient.ProcessDueRemindersNowAsync();
            TempData["SuccessMessage"] = response.Message;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Manual reminder processing failed.");
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/app/reminders/{reminderId:guid}/mark-read")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(Guid reminderId)
    {
        await _apiClient.MarkReminderReadAsync(reminderId);
        TempData["SuccessMessage"] = "Reminder marked as read.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/app/reminders/{reminderId:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid reminderId)
    {
        await _apiClient.DeleteReminderAsync(reminderId);
        TempData["SuccessMessage"] = "Reminder deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ReturnIndexViewAsync(ReminderFormViewModel form)
    {
        var reminders = await _apiClient.RemindersAsync();
        return View("Index", new RemindersPageViewModel
        {
            Form = form,
            Reminders = reminders
        });
    }

    private static List<ReminderChannel> ExtractSelectedChannels(ReminderFormViewModel model)
    {
        var channels = new List<ReminderChannel>();

        if (model.ChannelInApp)
        {
            channels.Add(ReminderChannel.InApp);
        }

        if (model.ChannelEmail)
        {
            channels.Add(ReminderChannel.Email);
        }

        if (model.ChannelBrowserPush)
        {
            channels.Add(ReminderChannel.BrowserPush);
        }

        return channels;
    }

    private static DateTime NormalizeToUtc(DateTime input)
    {
        return input.Kind switch
        {
            DateTimeKind.Utc => input,
            DateTimeKind.Local => input.ToUniversalTime(),
            _ => DateTime.SpecifyKind(input, DateTimeKind.Local).ToUniversalTime()
        };
    }
}