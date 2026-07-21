using System.Security.Claims;
using JiraTrack.Models.DTOs.Reports;
using JiraTrack.Models.DTOs.Sprints;
using JiraTrack.Repository.Interfaces;
using JiraTrack.Settings;

namespace JiraTrack.BusinessLogic;

public class ReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly SprintService _sprintService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ReportService(
        IUnitOfWork unitOfWork,
        SprintService sprintService,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _sprintService = sprintService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<DeveloperReportDto> GetDeveloperReportAsync(ReportFilterRequest filter, CancellationToken cancellationToken = default)
    {
        await EnsureReportManageAccessAsync(filter.ProjectId, cancellationToken);
        var userId = filter.UserId ?? throw new BusinessException("userId is required.", 400);
        var projectIds = await GetAccessibleProjectIdsAsync(cancellationToken);

        if (filter.ProjectId.HasValue && !projectIds.Contains(filter.ProjectId.Value))
            throw new ForbiddenBusinessException("You do not have access to this project.");

        return await _unitOfWork.Reports.GetDeveloperReportAsync(userId, projectIds, filter, cancellationToken);
    }

    public async Task<BugReportDto> GetBugReportAsync(ReportFilterRequest filter, CancellationToken cancellationToken = default)
    {
        await EnsureReportReadAccessAsync(filter.ProjectId, cancellationToken);
        var projectIds = await GetAccessibleProjectIdsAsync(cancellationToken);
        return await _unitOfWork.Reports.GetBugReportAsync(projectIds, filter, cancellationToken);
    }

    public async Task<SprintReportDto> GetSprintReportAsync(int sprintId, int projectId, CancellationToken cancellationToken = default)
    {
        await EnsureReportManageAccessAsync(projectId, cancellationToken);
        var project = await _unitOfWork.Projects.GetByIdAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Project not found.");

        var sprint = await _unitOfWork.Sprints.GetByIdAsync(sprintId, cancellationToken)
            ?? throw new NotFoundException("Sprint not found.");
        if (sprint.ProjectId != projectId) throw new NotFoundException("Sprint not found.");

        var velocity = await _sprintService.GetVelocityAsync(projectId, sprintId, cancellationToken);
        var burndown = await _sprintService.GetBurndownAsync(projectId, sprintId, cancellationToken);

        return new SprintReportDto
        {
            SprintId = sprintId,
            ProjectId = projectId,
            ProjectName = project.Name,
            SprintName = sprint.Name,
            SprintStatus = sprint.Status.ToString(),
            Velocity = velocity,
            Burndown = burndown
        };
    }

    public async Task<ProjectReportDto> GetProjectReportAsync(int projectId, CancellationToken cancellationToken = default)
    {
        await EnsureReportManageAccessAsync(projectId, cancellationToken);
        if (!await HasProjectAccessAsync(projectId, cancellationToken))
            throw new ForbiddenBusinessException("You do not have access to this project.");

        return await _unitOfWork.Reports.GetProjectReportAsync(projectId, cancellationToken);
    }

    public async Task<TimeTrackingReportDto> GetTimeTrackingReportAsync(ReportFilterRequest filter, CancellationToken cancellationToken = default)
    {
        await EnsureReportManageAccessAsync(filter.ProjectId, cancellationToken);
        var projectIds = await GetAccessibleProjectIdsAsync(cancellationToken);
        return await _unitOfWork.Reports.GetTimeTrackingReportAsync(projectIds, filter, cancellationToken);
    }

    public async Task<byte[]> ExportDeveloperPdfAsync(ReportFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var report = await GetDeveloperReportAsync(filter, cancellationToken);
        return ReportExportHelper.ExportDeveloperPdf(report, filter);
    }

    public async Task<byte[]> ExportDeveloperExcelAsync(ReportFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var report = await GetDeveloperReportAsync(filter, cancellationToken);
        return ReportExportHelper.ExportDeveloperExcel(report, filter);
    }

    public async Task<byte[]> ExportBugPdfAsync(ReportFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var report = await GetBugReportAsync(filter, cancellationToken);
        return ReportExportHelper.ExportBugPdf(report, filter);
    }

    public async Task<byte[]> ExportBugExcelAsync(ReportFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var report = await GetBugReportAsync(filter, cancellationToken);
        return ReportExportHelper.ExportBugExcel(report, filter);
    }

    private async Task<List<int>> GetAccessibleProjectIdsAsync(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdmin();
        return await _unitOfWork.Dashboard.GetAccessibleProjectIdsAsync(userId, isAdmin, cancellationToken);
    }

    private async Task<bool> HasProjectAccessAsync(int projectId, CancellationToken cancellationToken)
    {
        if (IsAdmin()) return true;
        return await _unitOfWork.Projects.IsMemberAsync(projectId, GetCurrentUserId(), cancellationToken);
    }

    private async Task EnsureReportManageAccessAsync(int? projectId, CancellationToken cancellationToken)
    {
        if (IsAdmin() || IsProjectManager()) return;
        throw new ForbiddenBusinessException("You do not have permission to access this report.");
    }

    private async Task EnsureReportReadAccessAsync(int? projectId, CancellationToken cancellationToken)
    {
        if (IsAdmin() || IsProjectManager() || IsQA()) return;
        throw new ForbiddenBusinessException("You do not have permission to access this report.");
    }

    private int GetCurrentUserId() =>
        int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.Admin);
    private bool IsProjectManager() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.ProjectManager);
    private bool IsQA() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.QA);
}
