using AIStudyPlanner.Api.Data;
using AIStudyPlanner.Api.DTOs.Auth;
using AIStudyPlanner.Api.Entities;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AIStudyPlanner.Api.Services;

public class AuthService(ApplicationDbContext dbContext, ITokenService tokenService) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await dbContext.Users.AnyAsync(x => x.Email == email))
        {
            throw new InvalidOperationException("An account with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return new AuthResponse
        {
            Token = tokenService.GenerateToken(user),
            User = Map(user)
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Email == email)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return new AuthResponse
        {
            Token = tokenService.GenerateToken(user),
            User = Map(user)
        };
    }

    public async Task<UserProfileResponse> GetCurrentUserAsync(Guid userId)
    {
        var user = await dbContext.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");
        return Map(user);
    }

    private static UserProfileResponse Map(User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email,
        CreatedAt = user.CreatedAt
    };
}
