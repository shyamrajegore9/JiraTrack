using System.Security.Claims;
using JiraTrack.Hubs;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Notifications;
using JiraTrack.Models.Entities;
using JiraTrack.Models.Enums;
using JiraTrack.Repository.Interfaces;
using JiraTrack.Settings;
using Microsoft.AspNetCore.SignalR;

namespace JiraTrack.BusinessLogic;

public class NotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NotificationService(
        IUnitOfWork unitOfWork,
        IHubContext<NotificationHub> hubContext,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResponse<NotificationDto>> GetNotificationsAsync(
        NotificationFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var result = await _unitOfWork.Notifications.GetPagedAsync(userId, filter, cancellationToken);

        return new PagedResponse<NotificationDto>
        {
            Items = result.Items.Select(MapToDto).ToList(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<UnreadCountDto> GetUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        var count = await _unitOfWork.Notifications.GetUnreadCountAsync(GetCurrentUserId(), cancellationToken);
        return new UnreadCountDto { Count = count };
    }

    public async Task MarkAsReadAsync(int id, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Notifications.MarkAsReadAsync(id, GetCurrentUserId(), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        await _unitOfWork.Notifications.MarkAllAsReadAsync(GetCurrentUserId(), cancellationToken);
    }

    public async Task NotifyAsync(
        int userId,
        NotificationType type,
        string title,
        string message,
        string? entityType = null,
        int? entityId = null,
        int? projectId = null,
        int? actorUserId = null,
        CancellationToken cancellationToken = default)
    {
        if (actorUserId.HasValue && actorUserId.Value == userId)
            return;

        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            EntityType = entityType,
            EntityId = entityId,
            ProjectId = projectId,
            IsRead = false,
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = MapToDto(notification);
        await _hubContext.Clients.Group($"user-{userId}").SendAsync("NotificationReceived", dto, cancellationToken);
    }

    public async Task NotifyManyAsync(
        IEnumerable<int> userIds,
        NotificationType type,
        string title,
        string message,
        string? entityType = null,
        int? entityId = null,
        int? projectId = null,
        int? actorUserId = null,
        CancellationToken cancellationToken = default)
    {
        foreach (var userId in userIds.Distinct())
            await NotifyAsync(userId, type, title, message, entityType, entityId, projectId, actorUserId, cancellationToken);
    }

    public async Task NotifyTaskAssignedAsync(TaskItem task, string actorName, int actorUserId, CancellationToken cancellationToken = default)
    {
        if (!task.AssigneeId.HasValue) return;

        await NotifyAsync(
            task.AssigneeId.Value,
            NotificationType.TaskAssigned,
            "Task assigned to you",
            $"{actorName} assigned task {task.TaskKey} to you",
            CommentEntityTypes.Task,
            task.Id,
            task.ProjectId,
            actorUserId,
            cancellationToken);
    }

    public async Task NotifyTaskUpdatedAsync(TaskItem task, string actorName, int actorUserId, CancellationToken cancellationToken = default)
    {
        if (!task.AssigneeId.HasValue) return;

        await NotifyAsync(
            task.AssigneeId.Value,
            NotificationType.TaskUpdated,
            "Task updated",
            $"{actorName} updated task {task.TaskKey}",
            CommentEntityTypes.Task,
            task.Id,
            task.ProjectId,
            actorUserId,
            cancellationToken);
    }

    public async Task NotifyTaskStatusChangedAsync(TaskItem task, string fromStatus, string actorName, int actorUserId, CancellationToken cancellationToken = default)
    {
        if (!task.AssigneeId.HasValue) return;

        await NotifyAsync(
            task.AssigneeId.Value,
            NotificationType.TaskStatusChanged,
            "Task status changed",
            $"{actorName} moved {task.TaskKey} from {fromStatus} to {task.Status}",
            CommentEntityTypes.Task,
            task.Id,
            task.ProjectId,
            actorUserId,
            cancellationToken);
    }

    public async Task NotifyBugAssignedAsync(Bug bug, string roleLabel, int assigneeId, string actorName, int actorUserId, CancellationToken cancellationToken = default)
    {
        await NotifyAsync(
            assigneeId,
            NotificationType.BugAssigned,
            "Bug assigned to you",
            $"{actorName} assigned you as {roleLabel} on {bug.BugKey}",
            CommentEntityTypes.Bug,
            bug.Id,
            bug.ProjectId,
            actorUserId,
            cancellationToken);
    }

    public async Task NotifyBugStatusChangedAsync(Bug bug, string fromStatus, string actorName, int actorUserId, CancellationToken cancellationToken = default)
    {
        var recipients = new List<int>();
        if (bug.DeveloperId.HasValue) recipients.Add(bug.DeveloperId.Value);
        if (bug.TesterId.HasValue && bug.TesterId != bug.DeveloperId) recipients.Add(bug.TesterId.Value);

        await NotifyManyAsync(
            recipients,
            NotificationType.BugStatusChanged,
            "Bug status changed",
            $"{actorName} moved {bug.BugKey} from {fromStatus} to {bug.Status}",
            CommentEntityTypes.Bug,
            bug.Id,
            bug.ProjectId,
            actorUserId,
            cancellationToken);
    }

    public async Task NotifyCommentAddedAsync(
        string entityType,
        int entityId,
        int projectId,
        string entityKey,
        IEnumerable<int> recipientIds,
        string actorName,
        int actorUserId,
        CancellationToken cancellationToken = default)
    {
        await NotifyManyAsync(
            recipientIds,
            NotificationType.CommentAdded,
            "New comment",
            $"{actorName} commented on {entityKey}",
            entityType,
            entityId,
            projectId,
            actorUserId,
            cancellationToken);
    }

    public async Task NotifyMentionAsync(
        int userId,
        string entityType,
        int entityId,
        int projectId,
        string entityKey,
        string actorName,
        int actorUserId,
        CancellationToken cancellationToken = default)
    {
        await NotifyAsync(
            userId,
            NotificationType.Mention,
            "You were mentioned",
            $"{actorName} mentioned you on {entityKey}",
            entityType,
            entityId,
            projectId,
            actorUserId,
            cancellationToken);
    }

    private int GetCurrentUserId() =>
        int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static NotificationDto MapToDto(Notification notification) => new()
    {
        Id = notification.Id,
        Type = notification.Type.ToString(),
        Title = notification.Title,
        Message = notification.Message,
        EntityType = notification.EntityType,
        EntityId = notification.EntityId,
        ProjectId = notification.ProjectId,
        IsRead = notification.IsRead,
        CreatedDate = notification.CreatedDate
    };
}
