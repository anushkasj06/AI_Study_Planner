using Microsoft.AspNetCore.Mvc;

namespace AIStudyPlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok",
            utc = DateTime.UtcNow
        });
    }
}
