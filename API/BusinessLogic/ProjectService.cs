using System.Security.Claims;
using AutoMapper;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Projects;
using JiraTrack.Models.Entities;
using JiraTrack.Repository.Interfaces;
using JiraTrack.Settings;

namespace JiraTrack.BusinessLogic;

public class ProjectService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ProjectService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<PagedResponse<ProjectListDto>> GetProjectsAsync(ProjectFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdmin();
        var result = await _unitOfWork.Projects.GetPagedAsync(filter, userId, isAdmin, cancellationToken);

        return new PagedResponse<ProjectListDto>
        {
            Items = result.Items.Select(MapToListDto).ToList(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<ProjectDetailDto> GetProjectByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(id, cancellationToken);
        var project = await _unitOfWork.Projects.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Project not found.");
        return MapToDetailDto(project);
    }

    public async Task<ProjectDetailDto> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        EnsureCanManageProjects();

        var key = request.Key.Trim().ToUpperInvariant();
        if (await _unitOfWork.Projects.GetByKeyAsync(key, cancellationToken) != null)
            throw new BusinessException("Project key already exists.", 409);

        var lead = await _unitOfWork.Users.GetByIdAsync(request.LeadUserId, cancellationToken)
            ?? throw new NotFoundException("Lead user not found.");

        var project = new Project
        {
            Key = key,
            Name = request.Name.Trim(),
            Description = request.Description,
            LeadUserId = lead.Id,
            IsArchived = false,
            CreatedBy = GetCurrentUserId(),
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.Projects.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _unitOfWork.Projects.AddMemberAsync(new ProjectMember
        {
            ProjectId = project.Id,
            UserId = lead.Id,
            ProjectRole = ProjectRoles.ProjectManager,
            JoinedDate = DateTime.UtcNow,
            CreatedBy = GetCurrentUserId(),
            CreatedDate = DateTime.UtcNow
        }, cancellationToken);

        var creatorId = GetCurrentUserId();
        if (creatorId != lead.Id)
        {
            await _unitOfWork.Projects.AddMemberAsync(new ProjectMember
            {
                ProjectId = project.Id,
                UserId = creatorId,
                ProjectRole = ProjectRoles.ProjectManager,
                JoinedDate = DateTime.UtcNow,
                CreatedBy = creatorId,
                CreatedDate = DateTime.UtcNow
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Project {ProjectKey} created by user {UserId}", key, creatorId);

        return await GetProjectByIdAsync(project.Id, cancellationToken);
    }

    public async Task<ProjectDetailDto> UpdateProjectAsync(int id, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectManageAccessAsync(id, cancellationToken);

        var project = await _unitOfWork.Projects.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Project not found.");

        var lead = await _unitOfWork.Users.GetByIdAsync(request.LeadUserId, cancellationToken)
            ?? throw new NotFoundException("Lead user not found.");

        project.Name = request.Name.Trim();
        project.Description = request.Description;
        project.LeadUserId = lead.Id;
        project.UpdatedBy = GetCurrentUserId();
        project.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Projects.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetProjectByIdAsync(id, cancellationToken);
    }

    public async Task DeleteProjectAsync(int id, CancellationToken cancellationToken = default)
    {
        await EnsureProjectManageAccessAsync(id, cancellationToken);

        var project = await _unitOfWork.Projects.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Project not found.");

        _unitOfWork.Projects.SoftDelete(project, GetCurrentUserId());
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ArchiveProjectAsync(int id, CancellationToken cancellationToken = default)
    {
        await EnsureProjectManageAccessAsync(id, cancellationToken);
        await SetArchivedAsync(id, true, cancellationToken);
    }

    public async Task UnarchiveProjectAsync(int id, CancellationToken cancellationToken = default)
    {
        await EnsureProjectManageAccessAsync(id, cancellationToken);
        await SetArchivedAsync(id, false, cancellationToken);
    }

    public async Task<ProjectDashboardDto> GetDashboardAsync(int id, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(id, cancellationToken);
        var project = await _unitOfWork.Projects.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Project not found.");

        var (total, open, _) = await _unitOfWork.Tasks.GetTaskCountsAsync(id, cancellationToken);
        var (totalBugs, openBugs) = await _unitOfWork.Bugs.GetBugCountsAsync(id, cancellationToken);
        var activeSprint = await _unitOfWork.Sprints.GetActiveSprintAsync(id, cancellationToken);

        return new ProjectDashboardDto
        {
            ProjectId = project.Id,
            Key = project.Key,
            Name = project.Name,
            TotalTasks = total,
            OpenTasks = open,
            TotalBugs = totalBugs,
            OpenBugs = openBugs,
            MemberCount = project.Members.Count,
            IsArchived = project.IsArchived,
            HasActiveSprint = activeSprint != null,
            ActiveSprintName = activeSprint?.Name
        };
    }

    public async Task<List<ProjectMemberDto>> GetMembersAsync(int projectId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, cancellationToken);
        var members = await _unitOfWork.Projects.GetMembersAsync(projectId, cancellationToken);
        return members.Select(m => new ProjectMemberDto
        {
            Id = m.Id,
            UserId = m.UserId,
            FullName = $"{m.User.FirstName} {m.User.LastName}".Trim(),
            Email = m.User.Email,
            ProjectRole = m.ProjectRole,
            JoinedDate = m.JoinedDate
        }).ToList();
    }

    public async Task<ProjectMemberDto> AddMemberAsync(int projectId, AddProjectMemberRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectManageAccessAsync(projectId, cancellationToken);

        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        var existing = await _unitOfWork.Projects.GetMemberAsync(projectId, request.UserId, cancellationToken);
        if (existing != null)
            throw new BusinessException("User is already a project member.", 409);

        var member = new ProjectMember
        {
            ProjectId = projectId,
            UserId = user.Id,
            ProjectRole = request.ProjectRole,
            JoinedDate = DateTime.UtcNow,
            CreatedBy = GetCurrentUserId(),
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.Projects.AddMemberAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ProjectMemberDto
        {
            Id = member.Id,
            UserId = user.Id,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            Email = user.Email,
            ProjectRole = member.ProjectRole,
            JoinedDate = member.JoinedDate
        };
    }

    public async Task RemoveMemberAsync(int projectId, int userId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectManageAccessAsync(projectId, cancellationToken);

        var member = await _unitOfWork.Projects.GetMemberAsync(projectId, userId, cancellationToken)
            ?? throw new NotFoundException("Project member not found.");

        var project = await _unitOfWork.Projects.GetByIdAsync(projectId, cancellationToken)
            ?? throw new NotFoundException("Project not found.");

        if (project.LeadUserId == userId)
            throw new BusinessException("Cannot remove the project lead. Assign a new lead first.");

        member.DeletedBy = GetCurrentUserId();
        _unitOfWork.Projects.RemoveMember(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProjectDetailDto> UpdateSettingsAsync(int id, UpdateProjectSettingsRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectManageAccessAsync(id, cancellationToken);

        var project = await _unitOfWork.Projects.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Project not found.");

        project.Name = request.Name.Trim();
        project.Description = request.Description;
        project.UpdatedBy = GetCurrentUserId();
        project.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Projects.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetProjectByIdAsync(id, cancellationToken);
    }

    private async Task SetArchivedAsync(int id, bool archived, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Project not found.");

        project.IsArchived = archived;
        project.UpdatedBy = GetCurrentUserId();
        project.UpdatedDate = DateTime.UtcNow;
        _unitOfWork.Projects.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureProjectAccessAsync(int projectId, CancellationToken cancellationToken)
    {
        if (IsAdmin()) return;

        if (!await _unitOfWork.Projects.IsMemberAsync(projectId, GetCurrentUserId(), cancellationToken))
            throw new ForbiddenBusinessException("You are not a member of this project.");
    }

    private async Task EnsureProjectManageAccessAsync(int projectId, CancellationToken cancellationToken)
    {
        if (IsAdmin()) return;

        if (!IsProjectManager())
            throw new ForbiddenBusinessException("Only administrators and project managers can perform this action.");

        if (!await _unitOfWork.Projects.IsMemberAsync(projectId, GetCurrentUserId(), cancellationToken))
            throw new ForbiddenBusinessException("You are not a member of this project.");
    }

    private void EnsureCanManageProjects()
    {
        if (!IsAdmin() && !IsProjectManager())
            throw new ForbiddenBusinessException("Only administrators and project managers can create projects.");
    }

    private int GetCurrentUserId() =>
        int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin() =>
        _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.Admin);

    private bool IsProjectManager() =>
        _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.ProjectManager);

    private static ProjectListDto MapToListDto(Project project) => new()
    {
        Id = project.Id,
        Key = project.Key,
        Name = project.Name,
        Description = project.Description,
        LeadName = $"{project.LeadUser.FirstName} {project.LeadUser.LastName}".Trim(),
        LeadUserId = project.LeadUserId,
        IsArchived = project.IsArchived,
        MemberCount = project.Members.Count,
        TaskCount = project.TaskCounter,
        BugCount = project.BugCounter,
        CreatedDate = project.CreatedDate
    };

    private static ProjectDetailDto MapToDetailDto(Project project)
    {
        var dto = new ProjectDetailDto
        {
            LeadEmail = project.LeadUser.Email
        };
        var list = MapToListDto(project);
        dto.Id = list.Id;
        dto.Key = list.Key;
        dto.Name = list.Name;
        dto.Description = list.Description;
        dto.LeadName = list.LeadName;
        dto.LeadUserId = list.LeadUserId;
        dto.IsArchived = list.IsArchived;
        dto.MemberCount = list.MemberCount;
        dto.TaskCount = list.TaskCount;
        dto.BugCount = list.BugCount;
        dto.CreatedDate = list.CreatedDate;
        return dto;
    }
}
