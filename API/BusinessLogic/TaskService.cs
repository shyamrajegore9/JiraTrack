using System.Security.Claims;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Tasks;
using JiraTrack.Models.Entities;
using JiraTrack.Models.Enums;
using JiraTrack.Repository.Interfaces;
using JiraTrack.Settings;

namespace JiraTrack.BusinessLogic;

public class TaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly KanbanService _kanbanService;
    private readonly NotificationService _notificationService;
    private readonly ILogger<TaskService> _logger;

    public TaskService(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        KanbanService kanbanService,
        NotificationService notificationService,
        ILogger<TaskService> logger)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _kanbanService = kanbanService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<PagedResponse<TaskListDto>> GetTasksAsync(int projectId, TaskFilterRequest filter, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, cancellationToken);
        var result = await _unitOfWork.Tasks.GetPagedAsync(projectId, filter, cancellationToken);
        return new PagedResponse<TaskListDto>
        {
            Items = result.Items.Select(MapToListDto).ToList(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<TaskDetailDto> GetTaskByIdAsync(int projectId, int taskId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, cancellationToken);
        var task = await _unitOfWork.Tasks.GetByIdWithDetailsAsync(projectId, taskId, cancellationToken)
            ?? throw new NotFoundException("Task not found.");
        return MapToDetailDto(task);
    }

    public async Task<TaskDetailDto> CreateTaskAsync(int projectId, CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectWriteAccessAsync(projectId, cancellationToken);

        if (request.ParentTaskId.HasValue)
        {
            var parent = await _unitOfWork.Tasks.GetByIdWithDetailsAsync(projectId, request.ParentTaskId.Value, cancellationToken)
                ?? throw new NotFoundException("Parent task not found.");
            if (parent.ParentTaskId.HasValue)
                throw new BusinessException("Subtasks cannot have nested subtasks.");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var taskKey = await _unitOfWork.Tasks.GenerateTaskKeyAsync(projectId, cancellationToken);
            var task = new TaskItem
            {
                ProjectId = projectId,
                TaskKey = taskKey,
                Title = request.Title.Trim(),
                Description = request.Description,
                AcceptanceCriteria = request.AcceptanceCriteria,
                Status = ParseStatus(request.Status),
                Priority = ParsePriority(request.Priority),
                AssigneeId = request.AssigneeId,
                ReporterId = GetCurrentUserId(),
                StoryPoints = request.StoryPoints,
                EstimatedHours = request.EstimatedHours,
                StartDate = request.StartDate,
                DueDate = request.DueDate,
                ParentTaskId = request.ParentTaskId,
                CreatedBy = GetCurrentUserId(),
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Tasks.AddAsync(task, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (request.LabelIds.Count > 0)
                await ValidateAndSetLabelsAsync(projectId, task.Id, request.LabelIds, cancellationToken);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            _logger.LogInformation("Task {TaskKey} created in project {ProjectId}", taskKey, projectId);

            if (!request.ParentTaskId.HasValue)
                await _kanbanService.NotifyCardAddedAsync(projectId, task, cancellationToken);

            if (task.AssigneeId.HasValue)
            {
                var actorUserId = GetCurrentUserId();
                var actorName = await GetActorNameAsync(actorUserId, cancellationToken);
                await _notificationService.NotifyTaskAssignedAsync(task, actorName, actorUserId, cancellationToken);
            }

            return await GetTaskByIdAsync(projectId, task.Id, cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<TaskDetailDto> UpdateTaskAsync(int projectId, int taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectWriteAccessAsync(projectId, cancellationToken);

        var task = await _unitOfWork.Tasks.GetByIdAsync(taskId, cancellationToken)
            ?? throw new NotFoundException("Task not found.");
        if (task.ProjectId != projectId) throw new NotFoundException("Task not found.");

        var previousAssigneeId = task.AssigneeId;
        var actorUserId = GetCurrentUserId();
        var actorName = await GetActorNameAsync(actorUserId, cancellationToken);

        task.Title = request.Title.Trim();
        task.Description = request.Description;
        task.AcceptanceCriteria = request.AcceptanceCriteria;
        task.Priority = ParsePriority(request.Priority);
        task.AssigneeId = request.AssigneeId;
        task.StoryPoints = request.StoryPoints;
        task.EstimatedHours = request.EstimatedHours;
        task.ActualHours = request.ActualHours;
        task.StartDate = request.StartDate;
        task.DueDate = request.DueDate;
        task.UpdatedBy = GetCurrentUserId();
        task.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.AssigneeId != previousAssigneeId && request.AssigneeId.HasValue)
            await _notificationService.NotifyTaskAssignedAsync(task, actorName, actorUserId, cancellationToken);
        else if (task.AssigneeId.HasValue)
            await _notificationService.NotifyTaskUpdatedAsync(task, actorName, actorUserId, cancellationToken);

        return await GetTaskByIdAsync(projectId, taskId, cancellationToken);
    }

    public async Task DeleteTaskAsync(int projectId, int taskId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectManageAccessAsync(projectId, cancellationToken);

        var task = await _unitOfWork.Tasks.GetByIdAsync(taskId, cancellationToken)
            ?? throw new NotFoundException("Task not found.");
        if (task.ProjectId != projectId) throw new NotFoundException("Task not found.");

        _unitOfWork.Tasks.SoftDelete(task, GetCurrentUserId());
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<TaskDetailDto> UpdateStatusAsync(int projectId, int taskId, UpdateTaskStatusRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectWriteAccessAsync(projectId, cancellationToken);

        var task = await _unitOfWork.Tasks.GetByIdAsync(taskId, cancellationToken)
            ?? throw new NotFoundException("Task not found.");
        if (task.ProjectId != projectId) throw new NotFoundException("Task not found.");

        var fromStatus = task.Status;
        task.Status = ParseStatus(request.Status);
        task.UpdatedBy = GetCurrentUserId();
        task.UpdatedDate = DateTime.UtcNow;
        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (!task.ParentTaskId.HasValue)
            await _kanbanService.NotifyCardMovedAsync(projectId, taskId, fromStatus.ToString(), task.Status.ToString(), task.SortOrder);

        var actorUserId = GetCurrentUserId();
        var actorName = await GetActorNameAsync(actorUserId, cancellationToken);
        await _notificationService.NotifyTaskStatusChangedAsync(task, fromStatus.ToString(), actorName, actorUserId, cancellationToken);

        return await GetTaskByIdAsync(projectId, taskId, cancellationToken);
    }

    public async Task<TaskDetailDto> AssignTaskAsync(int projectId, int taskId, AssignTaskRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectManageAccessAsync(projectId, cancellationToken);

        var task = await _unitOfWork.Tasks.GetByIdAsync(taskId, cancellationToken)
            ?? throw new NotFoundException("Task not found.");
        if (task.ProjectId != projectId) throw new NotFoundException("Task not found.");

        if (request.AssigneeId.HasValue)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(request.AssigneeId.Value, cancellationToken)
                ?? throw new NotFoundException("Assignee not found.");
        }

        task.AssigneeId = request.AssigneeId;
        task.UpdatedBy = GetCurrentUserId();
        task.UpdatedDate = DateTime.UtcNow;
        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.AssigneeId.HasValue)
        {
            var actorUserId = GetCurrentUserId();
            var actorName = await GetActorNameAsync(actorUserId, cancellationToken);
            await _notificationService.NotifyTaskAssignedAsync(task, actorName, actorUserId, cancellationToken);
        }

        return await GetTaskByIdAsync(projectId, taskId, cancellationToken);
    }

    public async Task<List<TaskListDto>> GetSubtasksAsync(int projectId, int taskId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, cancellationToken);
        var subtasks = await _unitOfWork.Tasks.GetSubtasksAsync(taskId, cancellationToken);
        return subtasks.Select(MapToListDto).ToList();
    }

    public async Task<TaskDetailDto> CreateSubtaskAsync(int projectId, int taskId, CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        request.ParentTaskId = taskId;
        return await CreateTaskAsync(projectId, request, cancellationToken);
    }

    public async Task<List<ChecklistItemDto>> GetChecklistAsync(int projectId, int taskId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, cancellationToken);
        await EnsureTaskExistsAsync(projectId, taskId, cancellationToken);
        var items = await _unitOfWork.Tasks.GetChecklistAsync(taskId, cancellationToken);
        return items.Select(MapChecklist).ToList();
    }

    public async Task<ChecklistItemDto> AddChecklistItemAsync(int projectId, int taskId, CreateChecklistItemRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectWriteAccessAsync(projectId, cancellationToken);
        await EnsureTaskExistsAsync(projectId, taskId, cancellationToken);

        var existing = await _unitOfWork.Tasks.GetChecklistAsync(taskId, cancellationToken);
        var item = new ChecklistItem
        {
            TaskId = taskId,
            Text = request.Text.Trim(),
            SortOrder = existing.Count,
            CreatedBy = GetCurrentUserId(),
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.Tasks.AddChecklistItemAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapChecklist(item);
    }

    public async Task<ChecklistItemDto> UpdateChecklistItemAsync(int projectId, int taskId, int itemId, UpdateChecklistItemRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectWriteAccessAsync(projectId, cancellationToken);
        var item = await _unitOfWork.Tasks.GetChecklistItemAsync(taskId, itemId, cancellationToken)
            ?? throw new NotFoundException("Checklist item not found.");

        item.Text = request.Text.Trim();
        item.IsCompleted = request.IsCompleted;
        item.UpdatedBy = GetCurrentUserId();
        item.UpdatedDate = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapChecklist(item);
    }

    public async Task DeleteChecklistItemAsync(int projectId, int taskId, int itemId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectWriteAccessAsync(projectId, cancellationToken);
        var item = await _unitOfWork.Tasks.GetChecklistItemAsync(taskId, itemId, cancellationToken)
            ?? throw new NotFoundException("Checklist item not found.");
        item.IsDeleted = true;
        item.DeletedBy = GetCurrentUserId();
        item.DeletedDate = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<TaskDetailDto> SetLabelsAsync(int projectId, int taskId, SetTaskLabelsRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectWriteAccessAsync(projectId, cancellationToken);
        await EnsureTaskExistsAsync(projectId, taskId, cancellationToken);
        await ValidateAndSetLabelsAsync(projectId, taskId, request.LabelIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetTaskByIdAsync(projectId, taskId, cancellationToken);
    }

    public async Task<List<LabelDto>> GetLabelsAsync(int projectId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, cancellationToken);
        var labels = await _unitOfWork.Tasks.GetProjectLabelsAsync(projectId, cancellationToken);
        return labels.Select(l => new LabelDto { Id = l.Id, Name = l.Name, Color = l.Color }).ToList();
    }

    public async Task<LabelDto> CreateLabelAsync(int projectId, CreateLabelRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectWriteAccessAsync(projectId, cancellationToken);

        var existing = (await _unitOfWork.Tasks.GetProjectLabelsAsync(projectId, cancellationToken))
            .FirstOrDefault(l => l.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
            throw new BusinessException("Label already exists.", 409);

        var label = new Label
        {
            ProjectId = projectId,
            Name = request.Name.Trim(),
            Color = request.Color,
            CreatedBy = GetCurrentUserId(),
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.Tasks.AddLabelAsync(label, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new LabelDto { Id = label.Id, Name = label.Name, Color = label.Color };
    }

    public async Task<TimeLogDto> AddTimeLogAsync(int projectId, int taskId, CreateTimeLogRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectWriteAccessAsync(projectId, cancellationToken);
        var task = await EnsureTaskExistsAsync(projectId, taskId, cancellationToken);

        var userId = GetCurrentUserId();
        var timeLog = new TimeLog
        {
            TaskId = taskId,
            UserId = userId,
            Hours = request.Hours,
            WorkDate = request.WorkDate.Date,
            Description = request.Description,
            CreatedBy = userId,
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.Tasks.AddTimeLogAsync(timeLog, cancellationToken);
        task.ActualHours += request.Hours;
        task.UpdatedBy = userId;
        task.UpdatedDate = DateTime.UtcNow;
        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        return new TimeLogDto
        {
            Id = timeLog.Id,
            UserId = userId,
            UserName = $"{user!.FirstName} {user.LastName}".Trim(),
            Hours = timeLog.Hours,
            WorkDate = timeLog.WorkDate,
            Description = timeLog.Description
        };
    }

    public async Task<List<TimeLogDto>> GetTimeLogsAsync(int projectId, int taskId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, cancellationToken);
        await EnsureTaskExistsAsync(projectId, taskId, cancellationToken);
        var logs = await _unitOfWork.Tasks.GetTimeLogsAsync(taskId, cancellationToken);
        return logs.Select(l => new TimeLogDto
        {
            Id = l.Id,
            UserId = l.UserId,
            UserName = $"{l.User.FirstName} {l.User.LastName}".Trim(),
            Hours = l.Hours,
            WorkDate = l.WorkDate,
            Description = l.Description
        }).ToList();
    }

    public async Task<(int Total, int Open, int Done)> GetTaskCountsAsync(int projectId, CancellationToken cancellationToken = default) =>
        await _unitOfWork.Tasks.GetTaskCountsAsync(projectId, cancellationToken);

    private async Task ValidateAndSetLabelsAsync(int projectId, int taskId, IEnumerable<int> labelIds, CancellationToken cancellationToken)
    {
        foreach (var labelId in labelIds.Distinct())
        {
            if (await _unitOfWork.Tasks.GetLabelByIdAsync(projectId, labelId, cancellationToken) == null)
                throw new BusinessException($"Invalid label ID: {labelId}");
        }
        await _unitOfWork.Tasks.SetLabelsAsync(taskId, labelIds, cancellationToken);
    }

    private async Task<TaskItem> EnsureTaskExistsAsync(int projectId, int taskId, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(taskId, cancellationToken)
            ?? throw new NotFoundException("Task not found.");
        if (task.ProjectId != projectId) throw new NotFoundException("Task not found.");
        return task;
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

    private int GetCurrentUserId() =>
        int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.Admin);
    private bool IsProjectManager() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.ProjectManager);
    private bool IsViewer() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.Viewer)
        && !IsAdmin() && !IsProjectManager();

    private async Task<string> GetActorNameAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        return user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Someone";
    }

    private static TaskItemStatus ParseStatus(string status) =>
        Enum.Parse<TaskItemStatus>(status, true);

    private static TaskPriority ParsePriority(string priority) =>
        Enum.Parse<TaskPriority>(priority, true);

    private static TaskListDto MapToListDto(TaskItem task) => new()
    {
        Id = task.Id,
        TaskKey = task.TaskKey,
        Title = task.Title,
        Status = task.Status.ToString(),
        Priority = task.Priority.ToString(),
        AssigneeId = task.AssigneeId,
        AssigneeName = task.Assignee != null ? $"{task.Assignee.FirstName} {task.Assignee.LastName}".Trim() : null,
        ReporterName = $"{task.Reporter.FirstName} {task.Reporter.LastName}".Trim(),
        StoryPoints = task.StoryPoints,
        DueDate = task.DueDate,
        Labels = task.TaskLabels.Select(tl => tl.Label.Name).ToList(),
        IsSubtask = task.ParentTaskId.HasValue,
        CreatedDate = task.CreatedDate
    };

    private static TaskDetailDto MapToDetailDto(TaskItem task)
    {
        var dto = new TaskDetailDto
        {
            Description = task.Description,
            AcceptanceCriteria = task.AcceptanceCriteria,
            EstimatedHours = task.EstimatedHours,
            ActualHours = task.ActualHours,
            StartDate = task.StartDate,
            ParentTaskId = task.ParentTaskId,
            ParentTaskKey = task.ParentTask?.TaskKey,
            Checklist = task.ChecklistItems.Select(MapChecklist).ToList(),
            Subtasks = task.Subtasks.Select(MapToListDto).ToList(),
            LabelDetails = task.TaskLabels.Select(tl => new LabelDto { Id = tl.Label.Id, Name = tl.Label.Name, Color = tl.Label.Color }).ToList(),
            TimeLogs = task.TimeLogs.Select(l => new TimeLogDto
            {
                Id = l.Id,
                UserId = l.UserId,
                UserName = $"{l.User.FirstName} {l.User.LastName}".Trim(),
                Hours = l.Hours,
                WorkDate = l.WorkDate,
                Description = l.Description
            }).ToList()
        };
        var list = MapToListDto(task);
        CopyListFields(list, dto);
        return dto;
    }

    private static void CopyListFields(TaskListDto source, TaskListDto target)
    {
        target.Id = source.Id;
        target.TaskKey = source.TaskKey;
        target.Title = source.Title;
        target.Status = source.Status;
        target.Priority = source.Priority;
        target.AssigneeId = source.AssigneeId;
        target.AssigneeName = source.AssigneeName;
        target.ReporterName = source.ReporterName;
        target.StoryPoints = source.StoryPoints;
        target.DueDate = source.DueDate;
        target.Labels = source.Labels;
        target.IsSubtask = source.IsSubtask;
        target.CreatedDate = source.CreatedDate;
    }

    private static ChecklistItemDto MapChecklist(ChecklistItem item) => new()
    {
        Id = item.Id,
        Text = item.Text,
        IsCompleted = item.IsCompleted,
        SortOrder = item.SortOrder
    };
}
