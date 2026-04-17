using AIStudyPlanner.Api.Data;
using AIStudyPlanner.Api.DTOs.Auth;
using AIStudyPlanner.Api.Entities;
using AIStudyPlanner.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AIStudyPlanner.Api.Services;

public class AuthService(ApplicationDbContext dbContext, ITokenService tokenService) : IAuthService
{
    private const int MaxFailedAttempts = 5;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await dbContext.Users.AnyAsync(x => x.Email == email))
        {
            throw new InvalidOperationException("An account with this email already exists.");
        }

        var phone = NormalizePhoneNumber(request.PhoneNumber);
        if (await dbContext.Users.AnyAsync(x => x.PhoneNumber == phone))
        {
            throw new InvalidOperationException("An account with this phone number already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Email = email,
            PhoneNumber = phone,
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

        if (user.LockedUntilUtc.HasValue && user.LockedUntilUtc.Value > DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Account temporarily locked due to multiple failed attempts. Please try again later.");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts += 1;

            if (user.FailedLoginAttempts >= MaxFailedAttempts)
            {
                var minutes = Math.Min(60, 5 * user.FailedLoginAttempts);
                user.LockedUntilUtc = DateTime.UtcNow.AddMinutes(minutes);
                user.FailedLoginAttempts = 0;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        user.FailedLoginAttempts = 0;
        user.LockedUntilUtc = null;
        user.LastLoginAtUtc = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

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
        PhoneNumber = user.PhoneNumber,
        LastLoginAtUtc = user.LastLoginAtUtc,
        CreatedAt = user.CreatedAt
    };

    private static string NormalizePhoneNumber(string rawPhone)
    {
        var normalized = rawPhone.Trim().Replace(" ", string.Empty).Replace("-", string.Empty);
        if (!normalized.StartsWith('+'))
        {
            normalized = $"+{normalized}";
        }

        return normalized;
    }
}
