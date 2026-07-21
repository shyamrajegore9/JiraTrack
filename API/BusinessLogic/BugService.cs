using System.Security.Claims;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Bugs;
using JiraTrack.Models.Entities;
using JiraTrack.Models.Enums;
using JiraTrack.Repository.Interfaces;
using JiraTrack.Settings;

namespace JiraTrack.BusinessLogic;

public class BugService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly NotificationService _notificationService;
    private readonly ILogger<BugService> _logger;

    public BugService(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        NotificationService notificationService,
        ILogger<BugService> logger)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<PagedResponse<BugListDto>> GetBugsAsync(int projectId, BugFilterRequest filter, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, cancellationToken);
        var result = await _unitOfWork.Bugs.GetPagedAsync(projectId, filter, cancellationToken);
        return new PagedResponse<BugListDto>
        {
            Items = result.Items.Select(MapToListDto).ToList(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<BugDetailDto> GetBugByIdAsync(int projectId, int bugId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, cancellationToken);
        var bug = await _unitOfWork.Bugs.GetByIdWithDetailsAsync(projectId, bugId, cancellationToken)
            ?? throw new NotFoundException("Bug not found.");
        return MapToDetailDto(bug);
    }

    public async Task<BugDetailDto> CreateBugAsync(int projectId, CreateBugRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectWriteAccessAsync(projectId, cancellationToken);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var bugKey = await _unitOfWork.Bugs.GenerateBugKeyAsync(projectId, cancellationToken);
            var bug = new Bug
            {
                ProjectId = projectId,
                BugKey = bugKey,
                Title = request.Title.Trim(),
                Description = request.Description,
                Severity = ParseSeverity(request.Severity),
                Priority = ParsePriority(request.Priority),
                Status = ParseStatus(request.Status),
                Environment = request.Environment,
                Browser = request.Browser,
                OperatingSystem = request.OperatingSystem,
                StepsToReproduce = request.StepsToReproduce,
                ExpectedResult = request.ExpectedResult,
                ActualResult = request.ActualResult,
                DeveloperId = request.DeveloperId,
                TesterId = request.TesterId,
                ReporterId = GetCurrentUserId(),
                CreatedBy = GetCurrentUserId(),
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Bugs.AddAsync(bug, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Bug {BugKey} created in project {ProjectId}", bugKey, projectId);

            var actorUserId = GetCurrentUserId();
            var actorName = await GetActorNameAsync(actorUserId, cancellationToken);
            if (bug.DeveloperId.HasValue)
                await _notificationService.NotifyBugAssignedAsync(bug, "developer", bug.DeveloperId.Value, actorName, actorUserId, cancellationToken);
            if (bug.TesterId.HasValue)
                await _notificationService.NotifyBugAssignedAsync(bug, "tester", bug.TesterId.Value, actorName, actorUserId, cancellationToken);

            return await GetBugByIdAsync(projectId, bug.Id, cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<BugDetailDto> UpdateBugAsync(int projectId, int bugId, UpdateBugRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectWriteAccessAsync(projectId, cancellationToken);

        var bug = await GetProjectBugAsync(projectId, bugId, cancellationToken);
        var previousDeveloperId = bug.DeveloperId;
        var previousTesterId = bug.TesterId;
        var actorUserId = GetCurrentUserId();
        var actorName = await GetActorNameAsync(actorUserId, cancellationToken);

        bug.Title = request.Title.Trim();
        bug.Description = request.Description;
        bug.Severity = ParseSeverity(request.Severity);
        bug.Priority = ParsePriority(request.Priority);
        bug.Environment = request.Environment;
        bug.Browser = request.Browser;
        bug.OperatingSystem = request.OperatingSystem;
        bug.StepsToReproduce = request.StepsToReproduce;
        bug.ExpectedResult = request.ExpectedResult;
        bug.ActualResult = request.ActualResult;
        bug.DeveloperId = request.DeveloperId;
        bug.TesterId = request.TesterId;
        bug.UpdatedBy = GetCurrentUserId();
        bug.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Bugs.Update(bug);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.DeveloperId != previousDeveloperId && request.DeveloperId.HasValue)
            await _notificationService.NotifyBugAssignedAsync(bug, "developer", request.DeveloperId.Value, actorName, actorUserId, cancellationToken);
        if (request.TesterId != previousTesterId && request.TesterId.HasValue)
            await _notificationService.NotifyBugAssignedAsync(bug, "tester", request.TesterId.Value, actorName, actorUserId, cancellationToken);

        return await GetBugByIdAsync(projectId, bugId, cancellationToken);
    }

    public async Task DeleteBugAsync(int projectId, int bugId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectManageAccessAsync(projectId, cancellationToken);

        var bug = await GetProjectBugAsync(projectId, bugId, cancellationToken);
        _unitOfWork.Bugs.SoftDelete(bug, GetCurrentUserId());
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<BugDetailDto> UpdateStatusAsync(int projectId, int bugId, UpdateBugStatusRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectWriteAccessAsync(projectId, cancellationToken);

        var bug = await GetProjectBugAsync(projectId, bugId, cancellationToken);
        var fromStatus = bug.Status;
        bug.Status = ParseStatus(request.Status);
        bug.UpdatedBy = GetCurrentUserId();
        bug.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Bugs.Update(bug);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var actorUserId = GetCurrentUserId();
        var actorName = await GetActorNameAsync(actorUserId, cancellationToken);
        await _notificationService.NotifyBugStatusChangedAsync(bug, fromStatus.ToString(), actorName, actorUserId, cancellationToken);

        return await GetBugByIdAsync(projectId, bugId, cancellationToken);
    }

    public async Task<BugDetailDto> AssignDeveloperAsync(int projectId, int bugId, AssignDeveloperRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectManageAccessAsync(projectId, cancellationToken);

        var bug = await GetProjectBugAsync(projectId, bugId, cancellationToken);
        bug.DeveloperId = request.DeveloperId;
        bug.UpdatedBy = GetCurrentUserId();
        bug.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Bugs.Update(bug);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.DeveloperId.HasValue)
        {
            var actorUserId = GetCurrentUserId();
            var actorName = await GetActorNameAsync(actorUserId, cancellationToken);
            await _notificationService.NotifyBugAssignedAsync(bug, "developer", request.DeveloperId.Value, actorName, actorUserId, cancellationToken);
        }

        return await GetBugByIdAsync(projectId, bugId, cancellationToken);
    }

    public async Task<BugDetailDto> AssignTesterAsync(int projectId, int bugId, AssignTesterRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureCanAssignTesterAsync(projectId, cancellationToken);

        var bug = await GetProjectBugAsync(projectId, bugId, cancellationToken);
        bug.TesterId = request.TesterId;
        bug.UpdatedBy = GetCurrentUserId();
        bug.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Bugs.Update(bug);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.TesterId.HasValue)
        {
            var actorUserId = GetCurrentUserId();
            var actorName = await GetActorNameAsync(actorUserId, cancellationToken);
            await _notificationService.NotifyBugAssignedAsync(bug, "tester", request.TesterId.Value, actorName, actorUserId, cancellationToken);
        }

        return await GetBugByIdAsync(projectId, bugId, cancellationToken);
    }

    private async Task<Bug> GetProjectBugAsync(int projectId, int bugId, CancellationToken cancellationToken)
    {
        var bug = await _unitOfWork.Bugs.GetByIdAsync(bugId, cancellationToken)
            ?? throw new NotFoundException("Bug not found.");
        if (bug.ProjectId != projectId) throw new NotFoundException("Bug not found.");
        return bug;
    }

    private async Task EnsureProjectAccessAsync(int projectId, CancellationToken cancellationToken)
    {
        if (IsAdmin()) return;
        if (!await _unitOfWork.Projects.IsMemberAsync(projectId, GetCurrentUserId(), cancellationToken))
            throw new ForbiddenBusinessException("You are not a member of this project.");
    }

    private async Task EnsureProjectWriteAccessAsync(int projectId, CancellationToken cancellationToken)
    {
        if (IsViewer()) throw new ForbiddenBusinessException("Viewers have read-only access.");
        await EnsureProjectAccessAsync(projectId, cancellationToken);
    }

    private async Task EnsureProjectManageAccessAsync(int projectId, CancellationToken cancellationToken)
    {
        if (IsAdmin()) return;
        if (!IsProjectManager())
            throw new ForbiddenBusinessException("Only administrators and project managers can perform this action.");
        await EnsureProjectAccessAsync(projectId, cancellationToken);
    }

    private async Task EnsureCanAssignTesterAsync(int projectId, CancellationToken cancellationToken)
    {
        if (IsAdmin() || IsProjectManager())
        {
            await EnsureProjectManageAccessAsync(projectId, cancellationToken);
            return;
        }

        if (IsQA())
        {
            await EnsureProjectWriteAccessAsync(projectId, cancellationToken);
            return;
        }

        throw new ForbiddenBusinessException("You do not have permission to assign testers.");
    }

    private int GetCurrentUserId() =>
        int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.Admin);
    private bool IsProjectManager() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.ProjectManager);
    private bool IsQA() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.QA);
    private bool IsViewer() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.Viewer)
        && !IsAdmin() && !IsProjectManager();

    private async Task<string> GetActorNameAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        return user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Someone";
    }

    private static BugItemStatus ParseStatus(string status) =>
        Enum.Parse<BugItemStatus>(status, true);

    private static BugSeverity ParseSeverity(string severity) =>
        Enum.Parse<BugSeverity>(severity, true);

    private static TaskPriority ParsePriority(string priority) =>
        Enum.Parse<TaskPriority>(priority, true);

    private static BugListDto MapToListDto(Bug bug) => new()
    {
        Id = bug.Id,
        BugKey = bug.BugKey,
        Title = bug.Title,
        Status = bug.Status.ToString(),
        Severity = bug.Severity.ToString(),
        Priority = bug.Priority.ToString(),
        DeveloperId = bug.DeveloperId,
        DeveloperName = bug.Developer != null ? $"{bug.Developer.FirstName} {bug.Developer.LastName}".Trim() : null,
        TesterId = bug.TesterId,
        TesterName = bug.Tester != null ? $"{bug.Tester.FirstName} {bug.Tester.LastName}".Trim() : null,
        ReporterName = $"{bug.Reporter.FirstName} {bug.Reporter.LastName}".Trim(),
        Environment = bug.Environment,
        CreatedDate = bug.CreatedDate
    };

    private static BugDetailDto MapToDetailDto(Bug bug)
    {
        var dto = new BugDetailDto
        {
            Description = bug.Description,
            Browser = bug.Browser,
            OperatingSystem = bug.OperatingSystem,
            StepsToReproduce = bug.StepsToReproduce,
            ExpectedResult = bug.ExpectedResult,
            ActualResult = bug.ActualResult
        };
        var list = MapToListDto(bug);
        dto.Id = list.Id;
        dto.BugKey = list.BugKey;
        dto.Title = list.Title;
        dto.Status = list.Status;
        dto.Severity = list.Severity;
        dto.Priority = list.Priority;
        dto.DeveloperId = list.DeveloperId;
        dto.DeveloperName = list.DeveloperName;
        dto.TesterId = list.TesterId;
        dto.TesterName = list.TesterName;
        dto.ReporterName = list.ReporterName;
        dto.Environment = list.Environment;
        dto.CreatedDate = list.CreatedDate;
        return dto;
    }
}
