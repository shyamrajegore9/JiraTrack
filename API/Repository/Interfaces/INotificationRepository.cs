using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Notifications;
using JiraTrack.Models.Entities;

namespace JiraTrack.Repository.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResponse<Notification>> GetPagedAsync(int userId, NotificationFilterRequest filter, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default);
    Task<Notification> AddAsync(Notification notification, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(int id, int userId, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);
}
