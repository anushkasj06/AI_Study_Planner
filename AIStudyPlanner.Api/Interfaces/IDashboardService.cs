using AIStudyPlanner.Api.DTOs.Dashboard;

namespace AIStudyPlanner.Api.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(Guid userId);
}
