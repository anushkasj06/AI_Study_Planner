using AIStudyPlanner.Web.Models;
using AIStudyPlanner.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Web.Controllers;

[Authorize]
public sealed class DashboardController : Controller
{
    private readonly StudyPlannerApiClient _apiClient;

    public DashboardController(StudyPlannerApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet("/app/dashboard")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var dashboard = await _apiClient.DashboardSummaryAsync();
            var progress = await _apiClient.ProgressSummaryAsync();

            return View(new DashboardPageViewModel
            {
                Dashboard = dashboard,
                Progress = progress
            });
        }
        catch (Exception exception)
        {
            TempData["ErrorMessage"] = exception.Message;
            return View(new DashboardPageViewModel());
        }
    }
}