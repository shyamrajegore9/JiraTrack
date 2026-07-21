using System.Security.Claims;
using JiraTrack.Models.DTOs.Sprints;
using JiraTrack.Models.Entities;
using JiraTrack.Models.Enums;
using JiraTrack.Repository.Interfaces;
using JiraTrack.Settings;

namespace JiraTrack.BusinessLogic;

public class SprintService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SprintService> _logger;

    public SprintService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILogger<SprintService> logger)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<List<SprintListDto>> GetSprintsAsync(int projectId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, cancellationToken);
        var sprints = await _unitOfWork.Sprints.GetByProjectAsync(projectId, cancellationToken);
        return sprints.Select(MapToListDto).ToList();
    }

    public async Task<SprintDetailDto> GetSprintByIdAsync(int projectId, int sprintId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, cancellationToken);
        var sprint = await _unitOfWork.Sprints.GetByIdWithTasksAsync(projectId, sprintId, cancellationToken)
            ?? throw new NotFoundException("Sprint not found.");
        return MapToDetailDto(sprint);
    }

    public async Task<SprintDetailDto> CreateSprintAsync(int projectId, CreateSprintRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectManageAccessAsync(projectId, cancellationToken);

        var sprint = new Sprint
        {
            ProjectId = projectId,
            Name = request.Name.Trim(),
            Goal = request.Goal,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = SprintStatus.Planning,
            CreatedBy = GetCurrentUserId(),
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.Sprints.AddAsync(sprint, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Sprint {SprintName} created in project {ProjectId}", sprint.Name, projectId);

        return await GetSprintByIdAsync(projectId, sprint.Id, cancellationToken);
    }

    public async Task<SprintDetailDto> UpdateSprintAsync(int projectId, int sprintId, UpdateSprintRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectManageAccessAsync(projectId, cancellationToken);

        var sprint = await GetProjectSprintAsync(projectId, sprintId, cancellationToken);
        if (sprint.Status != SprintStatus.Planning)
            throw new BusinessException("Only sprints in Planning status can be edited.", 409);

        sprint.Name = request.Name.Trim();
        sprint.Goal = request.Goal;
        sprint.StartDate = request.StartDate;
        sprint.EndDate = request.EndDate;
        sprint.UpdatedBy = GetCurrentUserId();
        sprint.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Sprints.Update(sprint);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetSprintByIdAsync(projectId, sprintId, cancellationToken);
    }

    public async Task DeleteSprintAsync(int projectId, int sprintId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectManageAccessAsync(projectId, cancellationToken);

        var sprint = await GetProjectSprintAsync(projectId, sprintId, cancellationToken);
        if (sprint.Status == SprintStatus.Active)
            throw new BusinessException("Cannot delete an active sprint.", 409);

        var tasks = await _unitOfWork.Sprints.GetSprintTasksAsync(sprintId, cancellationToken);
        foreach (var task in tasks)
        {
            task.SprintId = null;
            _unitOfWork.Tasks.Update(task);
        }

        _unitOfWork.Sprints.SoftDelete(sprint, GetCurrentUserId());
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<SprintDetailDto> StartSprintAsync(int projectId, int sprintId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectManageAccessAsync(projectId, cancellationToken);

        var sprint = await GetProjectSprintAsync(projectId, sprintId, cancellationToken);
        if (sprint.Status != SprintStatus.Planning)
            throw new BusinessException("Only sprints in Planning status can be started.", 409);

        if (await _unitOfWork.Sprints.HasActiveSprintAsync(projectId, sprintId, cancellationToken))
            throw new BusinessException("Another sprint is already active for this project.", 409);

        sprint.Status = SprintStatus.Active;
        sprint.StartDate ??= DateTime.UtcNow;
        sprint.UpdatedBy = GetCurrentUserId();
        sprint.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Sprints.Update(sprint);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Sprint {SprintId} started in project {ProjectId}", sprintId, projectId);

        return await GetSprintByIdAsync(projectId, sprintId, cancellationToken);
    }

    public async Task<SprintDetailDto> CloseSprintAsync(int projectId, int sprintId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectManageAccessAsync(projectId, cancellationToken);

        var sprint = await GetProjectSprintAsync(projectId, sprintId, cancellationToken);
        if (sprint.Status != SprintStatus.Active)
            throw new BusinessException("Only active sprints can be closed.", 409);

        var tasks = await _unitOfWork.Sprints.GetSprintTasksAsync(sprintId, cancellationToken);
        foreach (var task in tasks.Where(t => t.Status != TaskItemStatus.Done))
        {
            task.SprintId = null;
            task.UpdatedBy = GetCurrentUserId();
            task.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.Tasks.Update(task);
        }

        sprint.Status = SprintStatus.Closed;
        sprint.EndDate ??= DateTime.UtcNow;
        sprint.UpdatedBy = GetCurrentUserId();
        sprint.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Sprints.Update(sprint);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Sprint {SprintId} closed in project {ProjectId}", sprintId, projectId);

        return await GetSprintByIdAsync(projectId, sprintId, cancellationToken);
    }

    public async Task<List<SprintBacklogTaskDto>> GetBacklogAsync(int projectId, int sprintId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, cancellationToken);
        await EnsureSprintExistsAsync(projectId, sprintId, cancellationToken);

        var tasks = await _unitOfWork.Sprints.GetBacklogTasksAsync(sprintId, cancellationToken);
        return tasks.Select(MapBacklogTask).ToList();
    }

    public async Task<SprintBacklogTaskDto> AddTaskToBacklogAsync(int projectId, int sprintId, AddTaskToSprintBacklogRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectManageAccessAsync(projectId, cancellationToken);

        var sprint = await GetProjectSprintAsync(projectId, sprintId, cancellationToken);
        if (sprint.Status != SprintStatus.Planning)
            throw new BusinessException("Backlog can only be modified while sprint is in Planning status.", 409);

        var task = await _unitOfWork.Tasks.GetByIdAsync(request.TaskId, cancellationToken)
            ?? throw new NotFoundException("Task not found.");
        if (task.ProjectId != projectId || task.ParentTaskId.HasValue)
            throw new NotFoundException("Task not found.");
        if (task.SprintId.HasValue)
            throw new BusinessException("Task is already assigned to a sprint.", 409);

        task.SprintId = sprintId;
        task.UpdatedBy = GetCurrentUserId();
        task.UpdatedDate = DateTime.UtcNow;
        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _unitOfWork.Sprints.GetBacklogTasksAsync(sprintId, cancellationToken);
        return MapBacklogTask(updated.First(t => t.Id == task.Id));
    }

    public async Task RemoveTaskFromBacklogAsync(int projectId, int sprintId, int taskId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectManageAccessAsync(projectId, cancellationToken);

        var sprint = await GetProjectSprintAsync(projectId, sprintId, cancellationToken);
        if (sprint.Status != SprintStatus.Planning)
            throw new BusinessException("Backlog can only be modified while sprint is in Planning status.", 409);

        var task = await _unitOfWork.Tasks.GetByIdAsync(taskId, cancellationToken)
            ?? throw new NotFoundException("Task not found.");
        if (task.SprintId != sprintId)
            throw new NotFoundException("Task not found in sprint backlog.");

        task.SprintId = null;
        task.UpdatedBy = GetCurrentUserId();
        task.UpdatedDate = DateTime.UtcNow;
        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<SprintVelocityDto> GetVelocityAsync(int projectId, int sprintId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, cancellationToken);
        var sprint = await GetProjectSprintAsync(projectId, sprintId, cancellationToken);
        var tasks = await _unitOfWork.Sprints.GetSprintTasksAsync(sprintId, cancellationToken);

        var totalPoints = tasks.Sum(t => t.StoryPoints ?? 0);
        var completedPoints = tasks.Where(t => t.Status == TaskItemStatus.Done).Sum(t => t.StoryPoints ?? 0);
        var completedTasks = tasks.Count(t => t.Status == TaskItemStatus.Done);

        return new SprintVelocityDto
        {
            SprintId = sprintId,
            SprintName = sprint.Name,
            CompletedStoryPoints = completedPoints,
            TotalStoryPoints = totalPoints,
            TotalTasks = tasks.Count,
            CompletedTasks = completedTasks,
            CompletionRate = tasks.Count == 0 ? 0 : Math.Round((decimal)completedTasks / tasks.Count * 100, 1)
        };
    }

    public async Task<BurndownDto> GetBurndownAsync(int projectId, int sprintId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, cancellationToken);
        var sprint = await GetProjectSprintAsync(projectId, sprintId, cancellationToken);
        var tasks = await _unitOfWork.Sprints.GetSprintTasksAsync(sprintId, cancellationToken);

        var totalPoints = tasks.Sum(t => t.StoryPoints ?? 0);
        var startDate = (sprint.StartDate ?? sprint.CreatedDate).Date;
        var endDate = (sprint.EndDate ?? DateTime.UtcNow).Date;
        if (endDate < startDate) endDate = startDate;

        var totalDays = Math.Max((endDate - startDate).Days, 1);
        var points = new List<BurndownPointDto>();

        for (var day = 0; day <= totalDays; day++)
        {
            var date = startDate.AddDays(day);
            var completedByDate = tasks
                .Where(t => t.Status == TaskItemStatus.Done)
                .Where(t => (t.UpdatedDate ?? t.CreatedDate).Date <= date)
                .Sum(t => t.StoryPoints ?? 0);

            points.Add(new BurndownPointDto
            {
                Date = date,
                IdealRemaining = Math.Round(totalPoints - (totalPoints * day / (decimal)totalDays), 1),
                ActualRemaining = Math.Max(0, totalPoints - completedByDate)
            });
        }

        return new BurndownDto
        {
            SprintId = sprintId,
            TotalStoryPoints = totalPoints,
            Points = points
        };
    }

    private async Task<Sprint> GetProjectSprintAsync(int projectId, int sprintId, CancellationToken cancellationToken)
    {
        var sprint = await _unitOfWork.Sprints.GetByIdAsync(sprintId, cancellationToken)
            ?? throw new NotFoundException("Sprint not found.");
        if (sprint.ProjectId != projectId) throw new NotFoundException("Sprint not found.");
        return sprint;
    }

    private async Task EnsureSprintExistsAsync(int projectId, int sprintId, CancellationToken cancellationToken)
    {
        _ = await GetProjectSprintAsync(projectId, sprintId, cancellationToken);
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
        await EnsureProjectAccessAsync(projectId, cancellationToken);
    }

    private int GetCurrentUserId() =>
        int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.Admin);
    private bool IsProjectManager() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.ProjectManager);

    private static SprintListDto MapToListDto(Sprint sprint)
    {
        var tasks = sprint.Tasks.Where(t => !t.IsDeleted && t.ParentTaskId == null).ToList();
        var totalPoints = tasks.Sum(t => t.StoryPoints ?? 0);
        var completedPoints = tasks.Where(t => t.Status == TaskItemStatus.Done).Sum(t => t.StoryPoints ?? 0);

        return new SprintListDto
        {
            Id = sprint.Id,
            Name = sprint.Name,
            Goal = sprint.Goal,
            Status = sprint.Status.ToString(),
            StartDate = sprint.StartDate,
            EndDate = sprint.EndDate,
            TaskCount = tasks.Count,
            TotalStoryPoints = totalPoints,
            CompletedStoryPoints = completedPoints,
            CreatedDate = sprint.CreatedDate
        };
    }

    private static SprintDetailDto MapToDetailDto(Sprint sprint)
    {
        var dto = new SprintDetailDto();
        var list = MapToListDto(sprint);
        CopyListFields(list, dto);
        dto.Backlog = sprint.Tasks
            .Where(t => !t.IsDeleted && t.ParentTaskId == null)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.CreatedDate)
            .Select(MapBacklogTask)
            .ToList();
        return dto;
    }

    private static void CopyListFields(SprintListDto source, SprintListDto target)
    {
        target.Id = source.Id;
        target.Name = source.Name;
        target.Goal = source.Goal;
        target.Status = source.Status;
        target.StartDate = source.StartDate;
        target.EndDate = source.EndDate;
        target.TaskCount = source.TaskCount;
        target.TotalStoryPoints = source.TotalStoryPoints;
        target.CompletedStoryPoints = source.CompletedStoryPoints;
        target.CreatedDate = source.CreatedDate;
    }

    private static SprintBacklogTaskDto MapBacklogTask(TaskItem task) => new()
    {
        Id = task.Id,
        TaskKey = task.TaskKey,
        Title = task.Title,
        Status = task.Status.ToString(),
        Priority = task.Priority.ToString(),
        AssigneeName = task.Assignee != null ? $"{task.Assignee.FirstName} {task.Assignee.LastName}".Trim() : null,
        StoryPoints = task.StoryPoints
    };
}
