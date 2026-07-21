using Asp.Versioning;
using JiraTrack.BusinessLogic;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraTrack.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService) => _dashboardService = dashboardService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<DashboardSummaryDto>>> GetDashboard(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetDashboardAsync(cancellationToken);
        return Ok(ApiResponse<DashboardSummaryDto>.Ok(result));
    }

    [HttpGet("my-tasks")]
    public async Task<ActionResult<ApiResponse<List<MyTaskWidgetDto>>>> GetMyTasks(
        [FromQuery] MyTasksFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetMyTasksAsync(filter, cancellationToken);
        return Ok(ApiResponse<List<MyTaskWidgetDto>>.Ok(result));
    }

    [HttpGet("recent-activity")]
    public async Task<ActionResult<ApiResponse<List<ActivityItemDto>>>> GetRecentActivity(
        [FromQuery] ActivityFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetRecentActivityAsync(filter, cancellationToken);
        return Ok(ApiResponse<List<ActivityItemDto>>.Ok(result));
    }

    [HttpGet("bug-summary")]
    public async Task<ActionResult<ApiResponse<BugSummaryDto>>> GetBugSummary(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetBugSummaryAsync(cancellationToken);
        return Ok(ApiResponse<BugSummaryDto>.Ok(result));
    }
}
