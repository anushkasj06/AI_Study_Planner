using AIStudyPlanner.Web.Models;
using AIStudyPlanner.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Web.Controllers;

[Authorize]
public sealed class DashboardController : Controller
{
    private readonly StudyPlannerApiClient _apiClient;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(StudyPlannerApiClient apiClient, ILogger<DashboardController> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    [HttpGet("/app/dashboard")]
    public async Task<IActionResult> Index()
    {
        try
        {
            var dashboard = await _apiClient.DashboardSummaryAsync();
            var progress = await _apiClient.ProgressSummaryAsync();
            var notifications = await _apiClient.NotificationsAsync(includeRead: false);

            AiProviderStatusResponse? providerStatus = null;
            try
            {
                providerStatus = await _apiClient.AiProviderStatusAsync();
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "AI provider status lookup failed.");
            }

            return View(new DashboardPageViewModel
            {
                Dashboard = dashboard,
                Progress = progress,
                Notifications = notifications.Take(5).ToList(),
                AiProviderStatus = providerStatus
            });
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Dashboard API aggregation failed.");
            TempData["ErrorMessage"] = exception.Message;
            return View(new DashboardPageViewModel());
        }
    }
}