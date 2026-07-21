using System.Security.Claims;
using JiraTrack.Hubs;
using JiraTrack.Models.DTOs.Kanban;
using JiraTrack.Models.Entities;
using JiraTrack.Models.Enums;
using JiraTrack.Repository.Interfaces;
using JiraTrack.Settings;
using Microsoft.AspNetCore.SignalR;

namespace JiraTrack.BusinessLogic;

public class KanbanService
{
    private static readonly TaskItemStatus[] ColumnOrder =
    [
        TaskItemStatus.Todo,
        TaskItemStatus.InProgress,
        TaskItemStatus.CodeReview,
        TaskItemStatus.Testing,
        TaskItemStatus.Done
    ];

    private static readonly Dictionary<TaskItemStatus, string> ColumnTitles = new()
    {
        [TaskItemStatus.Todo] = "To Do",
        [TaskItemStatus.InProgress] = "In Progress",
        [TaskItemStatus.CodeReview] = "Code Review",
        [TaskItemStatus.Testing] = "Testing",
        [TaskItemStatus.Done] = "Done"
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHubContext<KanbanHub> _hubContext;
    private readonly ILogger<KanbanService> _logger;

    public KanbanService(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IHubContext<KanbanHub> hubContext,
        ILogger<KanbanService> logger)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<KanbanBoardDto> GetBoardAsync(int projectId, KanbanFilterRequest filter, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, cancellationToken);
        var tasks = await _unitOfWork.Tasks.GetKanbanTasksAsync(projectId, filter, cancellationToken);
        var grouped = tasks.GroupBy(t => t.Status).ToDictionary(g => g.Key, g => g.OrderBy(t => t.SortOrder).ThenBy(t => t.CreatedDate).ToList());

        var columns = ColumnOrder.Select(status => new KanbanColumnDto
        {
            Status = status.ToString(),
            Title = ColumnTitles[status],
            Count = grouped.TryGetValue(status, out var list) ? list.Count : 0,
            Cards = grouped.TryGetValue(status, out list) ? list.Select(MapToCard).ToList() : []
        }).ToList();

        return new KanbanBoardDto { ProjectId = projectId, Columns = columns };
    }

    public async Task<KanbanCardDto> MoveCardAsync(int projectId, MoveKanbanCardRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectWriteAccessAsync(projectId, cancellationToken);

        var task = await _unitOfWork.Tasks.GetByIdAsync(request.TaskId, cancellationToken)
            ?? throw new NotFoundException("Task not found.");
        if (task.ProjectId != projectId || task.ParentTaskId.HasValue)
            throw new NotFoundException("Task not found.");

        var fromStatus = task.Status;
        var toStatus = ParseStatus(request.ToStatus);
        task.Status = toStatus;
        task.SortOrder = request.NewSortOrder;
        task.UpdatedBy = GetCurrentUserId();
        task.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var detailed = await _unitOfWork.Tasks.GetByIdWithDetailsAsync(projectId, task.Id, cancellationToken)
            ?? task;
        var card = MapToCard(detailed);
        await BroadcastCardMovedAsync(projectId, request.TaskId, fromStatus.ToString(), toStatus.ToString(), request.NewSortOrder);
        _logger.LogInformation("Kanban card {TaskKey} moved from {From} to {To} in project {ProjectId}", task.TaskKey, fromStatus, toStatus, projectId);

        return card;
    }

    public async Task ReorderCardsAsync(int projectId, ReorderKanbanCardsRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectWriteAccessAsync(projectId, cancellationToken);

        var status = ParseStatus(request.Status);
        await _unitOfWork.Tasks.UpdateSortOrdersAsync(projectId, status, request.TaskIds, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await BroadcastCardUpdatedAsync(projectId, request.Status, request.TaskIds);
        _logger.LogInformation("Kanban cards reordered in column {Status} for project {ProjectId}", request.Status, projectId);
    }

    public async Task NotifyCardMovedAsync(int projectId, int taskId, string fromStatus, string toStatus, int sortOrder) =>
        await BroadcastCardMovedAsync(projectId, taskId, fromStatus, toStatus, sortOrder);

    public async Task NotifyCardAddedAsync(int projectId, TaskItem task, CancellationToken cancellationToken = default)
    {
        if (task.ParentTaskId.HasValue) return;

        var detailed = await _unitOfWork.Tasks.GetByIdWithDetailsAsync(projectId, task.Id, cancellationToken);
        if (detailed == null) return;

        await _hubContext.Clients.Group(GetGroupName(projectId)).SendAsync("CardAdded", new KanbanCardAddedEvent
        {
            ProjectId = projectId,
            Card = MapToCard(detailed)
        }, cancellationToken);
    }

    private async Task BroadcastCardMovedAsync(int projectId, int taskId, string fromStatus, string toStatus, int sortOrder)
    {
        await _hubContext.Clients.Group(GetGroupName(projectId)).SendAsync("CardMoved", new KanbanCardMovedEvent
        {
            ProjectId = projectId,
            TaskId = taskId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            SortOrder = sortOrder
        });
    }

    private async Task BroadcastCardUpdatedAsync(int projectId, string status, List<int> taskIds)
    {
        await _hubContext.Clients.Group(GetGroupName(projectId)).SendAsync("CardUpdated", new KanbanCardUpdatedEvent
        {
            ProjectId = projectId,
            Status = status,
            TaskIds = taskIds
        });
    }

    private static string GetGroupName(int projectId) => $"project-{projectId}";

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

    private int GetCurrentUserId() =>
        int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.Admin);
    private bool IsProjectManager() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.ProjectManager);
    private bool IsViewer() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.Viewer)
        && !IsAdmin() && !IsProjectManager();

    private static TaskItemStatus ParseStatus(string status) =>
        Enum.Parse<TaskItemStatus>(status, true);

    private static KanbanCardDto MapToCard(TaskItem task) => new()
    {
        Id = task.Id,
        TaskKey = task.TaskKey,
        Title = task.Title,
        Status = task.Status.ToString(),
        Priority = task.Priority.ToString(),
        AssigneeId = task.AssigneeId,
        AssigneeName = task.Assignee != null ? $"{task.Assignee.FirstName} {task.Assignee.LastName}".Trim() : null,
        Labels = task.TaskLabels.Select(tl => tl.Label.Name).ToList(),
        StoryPoints = task.StoryPoints,
        SortOrder = task.SortOrder
    };
}
