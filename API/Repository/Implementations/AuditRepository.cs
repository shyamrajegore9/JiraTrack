using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Audit;
using JiraTrack.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JiraTrack.Repository.Implementations;

public class AuditRepository : IAuditRepository
{
    private readonly ApplicationDbContext _context;

    public AuditRepository(ApplicationDbContext context) => _context = context;

    public async Task<PagedResponse<AuditLogListItemDto>> GetPagedAsync(
        AuditFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.EntityType))
            query = query.Where(a => a.EntityType == filter.EntityType);

        if (filter.EntityId.HasValue)
            query = query.Where(a => a.EntityId == filter.EntityId.Value);

        if (filter.UserId.HasValue)
            query = query.Where(a => a.UserId == filter.UserId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Action))
            query = query.Where(a => a.Action == filter.Action);

        if (filter.StartDate.HasValue)
            query = query.Where(a => a.Timestamp >= filter.StartDate.Value);

        if (filter.EndDate.HasValue)
            query = query.Where(a => a.Timestamp <= filter.EndDate.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        var pageNumber = filter.PageNumber < 1 ? 1 : filter.PageNumber;
        var pageSize = filter.PageSize is < 1 or > 100 ? 20 : filter.PageSize;

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .ThenByDescending(a => a.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AuditLogListItemDto
            {
                Id = a.Id,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Action = a.Action,
                UserId = a.UserId,
                UserName = a.User != null ? a.User.FirstName + " " + a.User.LastName : null,
                IpAddress = a.IpAddress,
                Timestamp = a.Timestamp
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<AuditLogListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<AuditLogDetailDto?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AuditLogDetailDto
            {
                Id = a.Id,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Action = a.Action,
                UserId = a.UserId,
                UserName = a.User != null ? a.User.FirstName + " " + a.User.LastName : null,
                IpAddress = a.IpAddress,
                Timestamp = a.Timestamp,
                OldValues = a.OldValues,
                NewValues = a.NewValues
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
