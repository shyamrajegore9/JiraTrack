using System.Security.Claims;
using Asp.Versioning;
using JiraTrack.BusinessLogic;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Projects;
using JiraTrack.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraTrack.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _projectService;

    public ProjectsController(ProjectService projectService) => _projectService = projectService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<ProjectListDto>>>> GetProjects(
        [FromQuery] ProjectFilterRequest filter,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.GetProjectsAsync(filter, cancellationToken);
        return Ok(ApiResponse<PagedResponse<ProjectListDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProjectDetailDto>>> GetProject(int id, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetProjectByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ProjectDetailDto>.Ok(result));
    }

    [HttpPost]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse<ProjectDetailDto>>> CreateProject(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.CreateProjectAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetProject), new { id = result.Id }, ApiResponse<ProjectDetailDto>.Ok(result, "Project created"));
    }

    [HttpPut("{id:int}")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse<ProjectDetailDto>>> UpdateProject(
        int id,
        [FromBody] UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.UpdateProjectAsync(id, request, cancellationToken);
        return Ok(ApiResponse<ProjectDetailDto>.Ok(result, "Project updated"));
    }

    [HttpDelete("{id:int}")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse>> DeleteProject(int id, CancellationToken cancellationToken)
    {
        await _projectService.DeleteProjectAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Project deleted"));
    }

    [HttpPatch("{id:int}/archive")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse>> ArchiveProject(int id, CancellationToken cancellationToken)
    {
        await _projectService.ArchiveProjectAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Project archived"));
    }

    [HttpPatch("{id:int}/unarchive")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse>> UnarchiveProject(int id, CancellationToken cancellationToken)
    {
        await _projectService.UnarchiveProjectAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Project unarchived"));
    }

    [HttpGet("{id:int}/dashboard")]
    public async Task<ActionResult<ApiResponse<ProjectDashboardDto>>> GetDashboard(int id, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetDashboardAsync(id, cancellationToken);
        return Ok(ApiResponse<ProjectDashboardDto>.Ok(result));
    }

    [HttpGet("{id:int}/members")]
    public async Task<ActionResult<ApiResponse<List<ProjectMemberDto>>>> GetMembers(int id, CancellationToken cancellationToken)
    {
        var result = await _projectService.GetMembersAsync(id, cancellationToken);
        return Ok(ApiResponse<List<ProjectMemberDto>>.Ok(result));
    }

    [HttpPost("{id:int}/members")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse<ProjectMemberDto>>> AddMember(
        int id,
        [FromBody] AddProjectMemberRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.AddMemberAsync(id, request, cancellationToken);
        return Ok(ApiResponse<ProjectMemberDto>.Ok(result, "Member added"));
    }

    [HttpDelete("{id:int}/members/{userId:int}")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse>> RemoveMember(int id, int userId, CancellationToken cancellationToken)
    {
        await _projectService.RemoveMemberAsync(id, userId, cancellationToken);
        return Ok(ApiResponse.Ok("Member removed"));
    }

    [HttpPut("{id:int}/settings")]
    [AuthorizeRoles(AppRoles.Admin, AppRoles.ProjectManager)]
    public async Task<ActionResult<ApiResponse<ProjectDetailDto>>> UpdateSettings(
        int id,
        [FromBody] UpdateProjectSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _projectService.UpdateSettingsAsync(id, request, cancellationToken);
        return Ok(ApiResponse<ProjectDetailDto>.Ok(result, "Settings updated"));
    }
}
