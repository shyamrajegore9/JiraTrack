using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Audit;

namespace JiraTrack.Repository.Interfaces;

public interface IAuditRepository
{
    Task<PagedResponse<AuditLogListItemDto>> GetPagedAsync(AuditFilterRequest filter, CancellationToken cancellationToken = default);
    Task<AuditLogDetailDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
}
