using AIStudyPlanner.Web.Models;
using AIStudyPlanner.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Web.Controllers;

[Authorize]
public sealed class ProfileController : Controller
{
    private readonly StudyPlannerApiClient _apiClient;

    public ProfileController(StudyPlannerApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [HttpGet("/app/profile")]
    public async Task<IActionResult> Index()
    {
        var user = await _apiClient.MeAsync();
        return View(new ProfilePageViewModel { User = user });
    }
}