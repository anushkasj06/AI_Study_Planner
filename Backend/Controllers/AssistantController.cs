using AIStudyPlanner.Api.DTOs.Assistant;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AssistantController(
    IStudyAssistantService assistantService,
    IStudyNoteService noteService,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpPost("chat")]
    public async Task<ActionResult<AssistantChatResponse>> Chat(AssistantChatRequest request, CancellationToken cancellationToken)
    {
        return Ok(await assistantService.ChatAsync(currentUserService.GetUserId(), request, cancellationToken));
    }

    [HttpGet("notes")]
    public async Task<ActionResult<IReadOnlyCollection<StudyNoteResponse>>> Notes()
    {
        return Ok(await noteService.GetNotesAsync(currentUserService.GetUserId()));
    }

    [HttpPost("notes")]
    public async Task<ActionResult<StudyNoteResponse>> CreateNote(CreateNoteFromAssistantRequest request, CancellationToken cancellationToken)
    {
        var created = await noteService.CreateFromAssistantAsync(currentUserService.GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(Notes), new { id = created.Id }, created);
    }

    [HttpDelete("notes/{noteId:guid}")]
    public async Task<IActionResult> DeleteNote(Guid noteId, CancellationToken cancellationToken)
    {
        await noteService.DeleteAsync(currentUserService.GetUserId(), noteId, cancellationToken);
        return NoContent();
    }
}
