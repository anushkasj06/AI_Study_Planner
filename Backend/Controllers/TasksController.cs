using AIStudyPlanner.Api.DTOs.Tasks;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TasksController(IPlanService planService, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("today")]
    public async Task<ActionResult<IReadOnlyCollection<StudyTaskResponse>>> Today()
    {
        return Ok(await planService.GetTodayTasksAsync(currentUserService.GetUserId()));
    }

    [HttpGet("week")]
    public async Task<ActionResult<IReadOnlyCollection<StudyTaskResponse>>> Week()
    {
        return Ok(await planService.GetWeekTasksAsync(currentUserService.GetUserId()));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StudyTaskResponse>> Update(Guid id, StudyTaskUpdateRequest request)
    {
        return Ok(await planService.UpdateTaskAsync(currentUserService.GetUserId(), id, request));
    }

    [HttpPatch("{id:guid}/toggle-complete")]
    public async Task<ActionResult<StudyTaskResponse>> ToggleComplete(Guid id)
    {
        return Ok(await planService.ToggleTaskCompletionAsync(currentUserService.GetUserId(), id));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await planService.DeleteTaskAsync(currentUserService.GetUserId(), id);
        return NoContent();
    }
}
