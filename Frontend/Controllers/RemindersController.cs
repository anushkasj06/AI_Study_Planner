using AIStudyPlanner.Web.Models;
using AIStudyPlanner.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Web.Controllers;

[Authorize]
public sealed class RemindersController : Controller
{
    private readonly StudyPlannerApiClient _apiClient;

    public RemindersController(StudyPlannerApiClient apiClient)
    {
        _apiClient = apiClient;
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

        await _apiClient.CreateReminderAsync(new ReminderCreateRequest
        {
            Title = model.Title,
            Message = model.Message,
            ReminderDateTime = reminderDateTime,
            Channel = model.Channel
        });

        TempData["SuccessMessage"] = "Reminder added.";
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
}