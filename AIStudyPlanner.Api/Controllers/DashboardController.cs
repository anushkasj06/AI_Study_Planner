using AIStudyPlanner.Api.DTOs.Dashboard;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DashboardController(IDashboardService dashboardService, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryResponse>> Summary()
    {
        return Ok(await dashboardService.GetSummaryAsync(currentUserService.GetUserId()));
    }
}
