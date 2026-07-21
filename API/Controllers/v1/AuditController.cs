using Asp.Versioning;
using JiraTrack.BusinessLogic;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Audit;
using JiraTrack.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraTrack.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/audit")]
[Authorize]
public class AuditController : ControllerBase
{
    private readonly AuditService _auditService;

    public AuditController(AuditService auditService) => _auditService = auditService;

    [HttpGet]
    [AuthorizeRoles(AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<PagedResponse<AuditLogListItemDto>>>> GetAuditLogs(
        [FromQuery] AuditFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _auditService.GetAuditLogsAsync(filter, cancellationToken);
        return Ok(ApiResponse<PagedResponse<AuditLogListItemDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    [AuthorizeRoles(AppRoles.Admin)]
    public async Task<ActionResult<ApiResponse<AuditLogDetailDto>>> GetAuditLog(
        long id, CancellationToken cancellationToken)
    {
        var result = await _auditService.GetAuditLogByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<AuditLogDetailDto>.Ok(result));
    }
}
