using JiraTrack.Models.DTOs.Dashboard;

namespace JiraTrack.Repository.Interfaces;

public interface IDashboardRepository
{
    Task<List<int>> GetAccessibleProjectIdsAsync(int userId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<DashboardSummaryDto> GetSummaryAsync(List<int> projectIds, CancellationToken cancellationToken = default);
    Task<List<MyTaskWidgetDto>> GetMyTasksAsync(int userId, List<int> projectIds, int limit, CancellationToken cancellationToken = default);
    Task<List<ActivityItemDto>> GetRecentActivityAsync(List<int> projectIds, int limit, CancellationToken cancellationToken = default);
    Task<BugSummaryDto> GetBugSummaryAsync(List<int> projectIds, CancellationToken cancellationToken = default);
}
