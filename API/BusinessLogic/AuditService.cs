using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Audit;
using JiraTrack.Repository.Interfaces;

namespace JiraTrack.BusinessLogic;

public class AuditService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public Task<PagedResponse<AuditLogListItemDto>> GetAuditLogsAsync(
        AuditFilterRequest filter, CancellationToken cancellationToken = default) =>
        _unitOfWork.Audit.GetPagedAsync(filter, cancellationToken);

    public async Task<AuditLogDetailDto> GetAuditLogByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var log = await _unitOfWork.Audit.GetByIdAsync(id, cancellationToken);
        if (log == null)
            throw new BusinessException("Audit log not found.", 404);

        return log;
    }
}
