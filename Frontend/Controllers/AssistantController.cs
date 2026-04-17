using AIStudyPlanner.Web.Models;
using AIStudyPlanner.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Web.Controllers;

[Authorize]
public sealed class AssistantController : Controller
{
    private readonly StudyPlannerApiClient _apiClient;

    public AssistantController(StudyPlannerApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpPost("/app/assistant/chat")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Chat([FromBody] AssistantChatRequest request)
    {
        var response = await _apiClient.AssistantChatAsync(request);
        return Json(response);
    }

    [HttpGet("/app/assistant/notes")]
    public async Task<IActionResult> Notes()
    {
        var notes = await _apiClient.AssistantNotesAsync();
        return Json(notes);
    }

    [HttpPost("/app/assistant/notes")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateNote([FromBody] CreateNoteFromAssistantRequest request)
    {
        var created = await _apiClient.CreateAssistantNoteAsync(request);
        return Json(created);
    }

    [HttpPost("/app/assistant/notes/{noteId:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNote(Guid noteId)
    {
        await _apiClient.DeleteAssistantNoteAsync(noteId);
        return NoContent();
    }
}
