using Asp.Versioning;
using JiraTrack.BusinessLogic;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Sprints;
using JiraTrack.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraTrack.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects/{projectId:int}/sprints")]
[Authorize]
public class SprintsController : ControllerBase
{
    private readonly SprintService _sprintService;

    public SprintsController(SprintService sprintService) => _sprintService = sprintService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<SprintListDto>>>> GetSprints(int projectId, CancellationToken cancellationToken)
    {
        var result = await _sprintService.GetSprintsAsync(projectId, cancellationToken);
        return Ok(ApiResponse<List<SprintListDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<SprintDetailDto>>> GetSprint(int projectId, int id, CancellationToken cancellationToken)
    {
        var result = await _sprintService.GetSprintByIdAsync(projectId, id, cancellationToken);
        return Ok(ApiResponse<SprintDetailDto>.Ok(result));
    }

    [HttpPost]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse<SprintDetailDto>>> CreateSprint(
        int projectId, [FromBody] CreateSprintRequest request, CancellationToken cancellationToken)
    {
        var result = await _sprintService.CreateSprintAsync(projectId, request, cancellationToken);
        return CreatedAtAction(nameof(GetSprint), new { projectId, id = result.Id }, ApiResponse<SprintDetailDto>.Ok(result, "Sprint created"));
    }

    [HttpPut("{id:int}")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse<SprintDetailDto>>> UpdateSprint(
        int projectId, int id, [FromBody] UpdateSprintRequest request, CancellationToken cancellationToken)
    {
        var result = await _sprintService.UpdateSprintAsync(projectId, id, request, cancellationToken);
        return Ok(ApiResponse<SprintDetailDto>.Ok(result, "Sprint updated"));
    }

    [HttpDelete("{id:int}")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse>> DeleteSprint(int projectId, int id, CancellationToken cancellationToken)
    {
        await _sprintService.DeleteSprintAsync(projectId, id, cancellationToken);
        return Ok(ApiResponse.Ok("Sprint deleted"));
    }

    [HttpPost("{id:int}/start")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse<SprintDetailDto>>> StartSprint(int projectId, int id, CancellationToken cancellationToken)
    {
        var result = await _sprintService.StartSprintAsync(projectId, id, cancellationToken);
        return Ok(ApiResponse<SprintDetailDto>.Ok(result, "Sprint started"));
    }

    [HttpPost("{id:int}/close")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse<SprintDetailDto>>> CloseSprint(int projectId, int id, CancellationToken cancellationToken)
    {
        var result = await _sprintService.CloseSprintAsync(projectId, id, cancellationToken);
        return Ok(ApiResponse<SprintDetailDto>.Ok(result, "Sprint closed"));
    }

    [HttpGet("{id:int}/backlog")]
    public async Task<ActionResult<ApiResponse<List<SprintBacklogTaskDto>>>> GetBacklog(int projectId, int id, CancellationToken cancellationToken)
    {
        var result = await _sprintService.GetBacklogAsync(projectId, id, cancellationToken);
        return Ok(ApiResponse<List<SprintBacklogTaskDto>>.Ok(result));
    }

    [HttpPost("{id:int}/backlog")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse<SprintBacklogTaskDto>>> AddToBacklog(
        int projectId, int id, [FromBody] AddTaskToSprintBacklogRequest request, CancellationToken cancellationToken)
    {
        var result = await _sprintService.AddTaskToBacklogAsync(projectId, id, request, cancellationToken);
        return Ok(ApiResponse<SprintBacklogTaskDto>.Ok(result, "Task added to sprint"));
    }

    [HttpDelete("{id:int}/backlog/{taskId:int}")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse>> RemoveFromBacklog(int projectId, int id, int taskId, CancellationToken cancellationToken)
    {
        await _sprintService.RemoveTaskFromBacklogAsync(projectId, id, taskId, cancellationToken);
        return Ok(ApiResponse.Ok("Task removed from sprint"));
    }

    [HttpGet("{id:int}/velocity")]
    public async Task<ActionResult<ApiResponse<SprintVelocityDto>>> GetVelocity(int projectId, int id, CancellationToken cancellationToken)
    {
        var result = await _sprintService.GetVelocityAsync(projectId, id, cancellationToken);
        return Ok(ApiResponse<SprintVelocityDto>.Ok(result));
    }

    [HttpGet("{id:int}/burndown")]
    public async Task<ActionResult<ApiResponse<BurndownDto>>> GetBurndown(int projectId, int id, CancellationToken cancellationToken)
    {
        var result = await _sprintService.GetBurndownAsync(projectId, id, cancellationToken);
        return Ok(ApiResponse<BurndownDto>.Ok(result));
    }
}
