using AIStudyPlanner.Api.DTOs.Auth;

namespace AIStudyPlanner.Api.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<UserProfileResponse> GetCurrentUserAsync(Guid userId);
}
