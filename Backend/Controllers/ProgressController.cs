using AIStudyPlanner.Api.DTOs.Progress;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProgressController(IProgressService progressService, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpPost("log")]
    public async Task<ActionResult<ProgressLogResponse>> Log(ProgressLogCreateRequest request)
    {
        return Ok(await progressService.LogProgressAsync(currentUserService.GetUserId(), request));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ProgressSummaryResponse>> Summary()
    {
        return Ok(await progressService.GetSummaryAsync(currentUserService.GetUserId()));
    }

    [HttpGet("goal/{goalId:guid}")]
    public async Task<ActionResult<GoalProgressResponse>> Goal(Guid goalId)
    {
        return Ok(await progressService.GetGoalProgressAsync(currentUserService.GetUserId(), goalId));
    }

    [HttpGet("streak")]
    public async Task<ActionResult<StreakResponse>> Streak()
    {
        return Ok(await progressService.GetStreakAsync(currentUserService.GetUserId()));
    }
}
