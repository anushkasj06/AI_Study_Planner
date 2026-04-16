using AIStudyPlanner.Api.DTOs.Plans;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AiController(IPlanService planService, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpPost("generate-plan/{goalId:guid}")]
    public async Task<ActionResult<StudyPlanResponse>> GeneratePlan(Guid goalId, CancellationToken cancellationToken)
    {
        return Ok(await planService.GeneratePlanAsync(currentUserService.GetUserId(), goalId, false, cancellationToken));
    }

    [HttpPost("regenerate-plan/{goalId:guid}")]
    public async Task<ActionResult<StudyPlanResponse>> RegeneratePlan(Guid goalId, CancellationToken cancellationToken)
    {
        return Ok(await planService.GeneratePlanAsync(currentUserService.GetUserId(), goalId, true, cancellationToken));
    }
}
