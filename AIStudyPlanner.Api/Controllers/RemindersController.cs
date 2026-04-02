using AIStudyPlanner.Api.DTOs.Reminders;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class RemindersController(IReminderService reminderService, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ReminderResponse>>> GetReminders()
    {
        return Ok(await reminderService.GetRemindersAsync(currentUserService.GetUserId()));
    }

    [HttpPost]
    public async Task<ActionResult<ReminderResponse>> Create(ReminderCreateRequest request)
    {
        var reminder = await reminderService.CreateReminderAsync(currentUserService.GetUserId(), request);
        return CreatedAtAction(nameof(GetReminders), new { id = reminder.Id }, reminder);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ReminderResponse>> Update(Guid id, ReminderUpdateRequest request)
    {
        return Ok(await reminderService.UpdateReminderAsync(currentUserService.GetUserId(), id, request));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await reminderService.DeleteReminderAsync(currentUserService.GetUserId(), id);
        return NoContent();
    }

    [HttpPatch("{id:guid}/mark-read")]
    public async Task<ActionResult<ReminderResponse>> MarkRead(Guid id)
    {
        return Ok(await reminderService.MarkReadAsync(currentUserService.GetUserId(), id));
    }
}
