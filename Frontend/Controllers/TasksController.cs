using AIStudyPlanner.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Web.Controllers;

[Authorize]
public sealed class TasksController : Controller
{
    private readonly StudyPlannerApiClient _apiClient;

    public TasksController(StudyPlannerApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpPost("/app/tasks/{taskId:guid}/toggle")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(Guid taskId, string returnUrl = "/app/planner")
    {
        await _apiClient.ToggleTaskAsync(taskId);
        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/app/planner" : returnUrl);
    }

    [HttpPost("/app/tasks/{taskId:guid}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid taskId, string returnUrl = "/app/planner")
    {
        await _apiClient.DeleteTaskAsync(taskId);
        return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/app/planner" : returnUrl);
    }
}