using AIStudyPlanner.Api.DTOs.Auth;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AIStudyPlanner.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        return Ok(await authService.RegisterAsync(request));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthLoginPolicy")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        return Ok(await authService.LoginAsync(request));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponse>> Me()
    {
        return Ok(await authService.GetCurrentUserAsync(currentUserService.GetUserId()));
    }
}
