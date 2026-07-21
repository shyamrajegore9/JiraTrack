using Asp.Versioning;
using JiraTrack.BusinessLogic;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraTrack.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationsController(NotificationService notificationService) =>
        _notificationService = notificationService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<NotificationDto>>>> GetNotifications(
        [FromQuery] NotificationFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _notificationService.GetNotificationsAsync(filter, cancellationToken);
        return Ok(ApiResponse<PagedResponse<NotificationDto>>.Ok(result));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<UnreadCountDto>>> GetUnreadCount(CancellationToken cancellationToken)
    {
        var result = await _notificationService.GetUnreadCountAsync(cancellationToken);
        return Ok(ApiResponse<UnreadCountDto>.Ok(result));
    }

    [HttpPatch("{id:int}/read")]
    public async Task<ActionResult<ApiResponse>> MarkAsRead(int id, CancellationToken cancellationToken)
    {
        await _notificationService.MarkAsReadAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Notification marked as read"));
    }

    [HttpPatch("read-all")]
    public async Task<ActionResult<ApiResponse>> MarkAllAsRead(CancellationToken cancellationToken)
    {
        await _notificationService.MarkAllAsReadAsync(cancellationToken);
        return Ok(ApiResponse.Ok("All notifications marked as read"));
    }
}
