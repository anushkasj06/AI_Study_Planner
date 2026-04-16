using AIStudyPlanner.Web.Models;
using AIStudyPlanner.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Web.Controllers;

[Authorize]
public sealed class ProgressController : Controller
{
    private readonly StudyPlannerApiClient _apiClient;

    public ProgressController(StudyPlannerApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet("/app/progress")]
    public async Task<IActionResult> Index()
    {
        var summary = await _apiClient.ProgressSummaryAsync();
        var goals = await _apiClient.GoalsAsync();

        return View(new ProgressPageViewModel
        {
            Summary = summary,
            Goals = goals
        });
    }
}