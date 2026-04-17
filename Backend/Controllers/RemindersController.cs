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

    [HttpPost("batch")]
    public async Task<ActionResult<IReadOnlyCollection<ReminderResponse>>> CreateBatch(
        ReminderBatchCreateRequest request,
        CancellationToken cancellationToken)
    {
        var reminders = await reminderService.CreateReminderBatchAsync(currentUserService.GetUserId(), request, cancellationToken);
        return Ok(reminders);
    }

    [HttpPost("ai/draft")]
    public async Task<ActionResult<AiReminderDraftResponse>> GenerateAiDraft(
        AiReminderDraftRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await reminderService.GenerateAiReminderDraftAsync(currentUserService.GetUserId(), request, cancellationToken));
    }

    [HttpPost("ai/sync")]
    public async Task<ActionResult<ReminderPipelineActionResponse>> SyncAiReminders(CancellationToken cancellationToken)
    {
        var created = await reminderService.GenerateSmartRemindersAsync(currentUserService.GetUserId(), cancellationToken);
        return Ok(new ReminderPipelineActionResponse
        {
            Count = created,
            Message = created == 0 ? "No new AI reminders were required." : $"AI sync created {created} reminder records."
        });
    }

    [HttpPost("process-now")]
    public async Task<ActionResult<ReminderPipelineActionResponse>> ProcessNow(CancellationToken cancellationToken)
    {
        var processed = await reminderService.ProcessDueRemindersForUserAsync(currentUserService.GetUserId(), cancellationToken);
        return Ok(new ReminderPipelineActionResponse
        {
            Count = processed,
            Message = processed == 0 ? "No due reminders right now." : $"Processed {processed} due reminder records."
        });
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
