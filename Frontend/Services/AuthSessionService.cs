using System.Security.Claims;
using AIStudyPlanner.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AIStudyPlanner.Web.Services;

public sealed class AuthSessionService
{
    public async Task SignInAsync(HttpContext context, AuthResponse authResponse)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, authResponse.User.Id.ToString()),
            new(ClaimTypes.Name, authResponse.User.FullName),
            new(ClaimTypes.Email, authResponse.User.Email),
            new("access_token", authResponse.Token),
            new("created_at", authResponse.User.CreatedAt.ToString("O"))
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
        };

        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
    }

    public Task SignOutAsync(HttpContext context)
    {
        return context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}