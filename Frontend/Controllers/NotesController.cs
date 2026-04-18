using AIStudyPlanner.Web.Models;
using AIStudyPlanner.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Web.Controllers;

[Authorize]
public sealed class NotesController : Controller
{
    private readonly StudyPlannerApiClient _apiClient;
    private readonly ILogger<NotesController> _logger;

    public NotesController(StudyPlannerApiClient apiClient, ILogger<NotesController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    [HttpGet("/app/notes")]
    public async Task<IActionResult> Index()
    {
        var notes = await _apiClient.AssistantNotesAsync();
        return View(new NotesPageViewModel
        {
            Notes = notes
        });
    }

    [HttpPost("/app/notes/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NotesPageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return await ReturnIndexViewAsync(model);
        }

        try
        {
            await _apiClient.CreateAssistantNoteAsync(new CreateNoteFromAssistantRequest
            {
                Prompt = model.Prompt.Trim()
            });

            TempData["SuccessMessage"] = "Note created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Note creation failed.");
            ModelState.AddModelError(string.Empty, exception.Message);
            return await ReturnIndexViewAsync(model);
        }
    }

    [HttpPost("/app/notes/{noteId:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid noteId)
    {
        try
        {
            await _apiClient.DeleteAssistantNoteAsync(noteId);
            TempData["SuccessMessage"] = "Note deleted.";
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Note delete failed for note {NoteId}", noteId);
            TempData["ErrorMessage"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ReturnIndexViewAsync(NotesPageViewModel model)
    {
        model.Notes = await _apiClient.AssistantNotesAsync();
        return View("Index", model);
    }
}