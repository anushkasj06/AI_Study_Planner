namespace AIStudyPlanner.Api.Interfaces;

public interface ICurrentUserService
{
    Guid GetUserId();
    string GetEmail();
}
