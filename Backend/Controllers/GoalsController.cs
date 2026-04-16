using AIStudyPlanner.Api.DTOs.Goals;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class GoalsController(IGoalService goalService, IPlanService planService, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<StudyGoalResponse>>> GetGoals()
    {
        return Ok(await goalService.GetGoalsAsync(currentUserService.GetUserId()));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudyGoalResponse>> GetGoal(Guid id)
    {
        return Ok(await goalService.GetGoalByIdAsync(currentUserService.GetUserId(), id));
    }

    [HttpPost]
    public async Task<ActionResult<StudyGoalResponse>> CreateGoal(StudyGoalCreateRequest request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetUserId();
        var goal = await goalService.CreateGoalAsync(userId, request);

        if (request.AutoGeneratePlan)
        {
            await planService.GeneratePlanAsync(userId, goal.Id, false, cancellationToken);
        }

        return CreatedAtAction(nameof(GetGoal), new { id = goal.Id }, goal);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StudyGoalResponse>> UpdateGoal(Guid id, StudyGoalUpdateRequest request)
    {
        return Ok(await goalService.UpdateGoalAsync(currentUserService.GetUserId(), id, request));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteGoal(Guid id)
    {
        await goalService.DeleteGoalAsync(currentUserService.GetUserId(), id);
        return NoContent();
    }
}
