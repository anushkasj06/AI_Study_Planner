using System.Security.Claims;
using AIStudyPlanner.Api.Interfaces;

namespace AIStudyPlanner.Api.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid GetUserId()
    {
        var claim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(claim, out var userId))
        {
            throw new UnauthorizedAccessException("User context is not available.");
        }

        return userId;
    }

    public string GetEmail()
    {
        return httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
            ?? throw new UnauthorizedAccessException("User email is not available.");
    }
}
