using AIStudyPlanner.Api.DTOs.Plans;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PlansController(IPlanService planService, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<StudyPlanResponse>>> GetPlans()
    {
        return Ok(await planService.GetPlansAsync(currentUserService.GetUserId()));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudyPlanResponse>> GetPlan(Guid id)
    {
        return Ok(await planService.GetPlanByIdAsync(currentUserService.GetUserId(), id));
    }

    [HttpGet("goal/{goalId:guid}")]
    public async Task<ActionResult<IReadOnlyCollection<StudyPlanResponse>>> GetByGoal(Guid goalId)
    {
        return Ok(await planService.GetPlansByGoalAsync(currentUserService.GetUserId(), goalId));
    }
}
