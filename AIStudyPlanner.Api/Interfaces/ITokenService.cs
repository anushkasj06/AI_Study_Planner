using AIStudyPlanner.Api.Entities;

namespace AIStudyPlanner.Api.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
