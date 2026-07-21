using Asp.Versioning;
using JiraTrack.BusinessLogic;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Bugs;
using JiraTrack.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraTrack.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects/{projectId:int}")]
[Authorize]
public class BugsController : ControllerBase
{
    private readonly BugService _bugService;

    public BugsController(BugService bugService) => _bugService = bugService;

    [HttpGet("bugs")]
    public async Task<ActionResult<ApiResponse<PagedResponse<BugListDto>>>> GetBugs(
        int projectId, [FromQuery] BugFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _bugService.GetBugsAsync(projectId, filter, cancellationToken);
        return Ok(ApiResponse<PagedResponse<BugListDto>>.Ok(result));
    }

    [HttpGet("bugs/{id:int}")]
    public async Task<ActionResult<ApiResponse<BugDetailDto>>> GetBug(int projectId, int id, CancellationToken cancellationToken)
    {
        var result = await _bugService.GetBugByIdAsync(projectId, id, cancellationToken);
        return Ok(ApiResponse<BugDetailDto>.Ok(result));
    }

    [HttpPost("bugs")]
    public async Task<ActionResult<ApiResponse<BugDetailDto>>> CreateBug(
        int projectId, [FromBody] CreateBugRequest request, CancellationToken cancellationToken)
    {
        var result = await _bugService.CreateBugAsync(projectId, request, cancellationToken);
        return CreatedAtAction(nameof(GetBug), new { projectId, id = result.Id }, ApiResponse<BugDetailDto>.Ok(result, "Bug created"));
    }

    [HttpPut("bugs/{id:int}")]
    public async Task<ActionResult<ApiResponse<BugDetailDto>>> UpdateBug(
        int projectId, int id, [FromBody] UpdateBugRequest request, CancellationToken cancellationToken)
    {
        var result = await _bugService.UpdateBugAsync(projectId, id, request, cancellationToken);
        return Ok(ApiResponse<BugDetailDto>.Ok(result, "Bug updated"));
    }

    [HttpDelete("bugs/{id:int}")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse>> DeleteBug(int projectId, int id, CancellationToken cancellationToken)
    {
        await _bugService.DeleteBugAsync(projectId, id, cancellationToken);
        return Ok(ApiResponse.Ok("Bug deleted"));
    }

    [HttpPatch("bugs/{id:int}/status")]
    public async Task<ActionResult<ApiResponse<BugDetailDto>>> UpdateStatus(
        int projectId, int id, [FromBody] UpdateBugStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _bugService.UpdateStatusAsync(projectId, id, request, cancellationToken);
        return Ok(ApiResponse<BugDetailDto>.Ok(result, "Status updated"));
    }

    [HttpPatch("bugs/{id:int}/assign-developer")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse<BugDetailDto>>> AssignDeveloper(
        int projectId, int id, [FromBody] AssignDeveloperRequest request, CancellationToken cancellationToken)
    {
        var result = await _bugService.AssignDeveloperAsync(projectId, id, request, cancellationToken);
        return Ok(ApiResponse<BugDetailDto>.Ok(result, "Developer assigned"));
    }

    [HttpPatch("bugs/{id:int}/assign-tester")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager, AppRoles.QA)]
    public async Task<ActionResult<ApiResponse<BugDetailDto>>> AssignTester(
        int projectId, int id, [FromBody] AssignTesterRequest request, CancellationToken cancellationToken)
    {
        var result = await _bugService.AssignTesterAsync(projectId, id, request, cancellationToken);
        return Ok(ApiResponse<BugDetailDto>.Ok(result, "Tester assigned"));
    }
}
