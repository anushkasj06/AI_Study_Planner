using AIStudyPlanner.Web.Models;
using AIStudyPlanner.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Web.Controllers;

[Authorize]
public sealed class PlannerController : Controller
{
    private readonly StudyPlannerApiClient _apiClient;

    public PlannerController(StudyPlannerApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet("/app/planner")]
    public async Task<IActionResult> Index()
    {
        var todayTasks = await _apiClient.TodayTasksAsync();
        var weekTasks = await _apiClient.WeekTasksAsync();

        return View(new PlannerPageViewModel
        {
            TodayTasks = todayTasks,
            WeekTasks = weekTasks
        });
    }
}