using System.Security.Claims;
using JiraTrack.Models.DTOs.Dashboard;
using JiraTrack.Repository.Interfaces;
using JiraTrack.Settings;

namespace JiraTrack.BusinessLogic;

public class DashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DashboardService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<DashboardSummaryDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var projectIds = await GetAccessibleProjectIdsAsync(cancellationToken);
        return await _unitOfWork.Dashboard.GetSummaryAsync(projectIds, cancellationToken);
    }

    public async Task<List<MyTaskWidgetDto>> GetMyTasksAsync(MyTasksFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var projectIds = await GetAccessibleProjectIdsAsync(cancellationToken);
        return await _unitOfWork.Dashboard.GetMyTasksAsync(userId, projectIds, filter.Limit, cancellationToken);
    }

    public async Task<List<ActivityItemDto>> GetRecentActivityAsync(ActivityFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var projectIds = await GetAccessibleProjectIdsAsync(cancellationToken);
        return await _unitOfWork.Dashboard.GetRecentActivityAsync(projectIds, filter.Limit, cancellationToken);
    }

    public async Task<BugSummaryDto> GetBugSummaryAsync(CancellationToken cancellationToken = default)
    {
        var projectIds = await GetAccessibleProjectIdsAsync(cancellationToken);
        return await _unitOfWork.Dashboard.GetBugSummaryAsync(projectIds, cancellationToken);
    }

    private async Task<List<int>> GetAccessibleProjectIdsAsync(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdmin();
        return await _unitOfWork.Dashboard.GetAccessibleProjectIdsAsync(userId, isAdmin, cancellationToken);
    }

    private int GetCurrentUserId() =>
        int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.Admin);
}
