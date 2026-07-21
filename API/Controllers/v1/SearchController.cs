using Asp.Versioning;
using JiraTrack.BusinessLogic;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraTrack.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/search")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly SearchService _searchService;

    public SearchController(SearchService searchService) => _searchService = searchService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<SearchResponseDto>>> Search(
        [FromQuery] SearchFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _searchService.SearchAllAsync(filter, cancellationToken);
        return Ok(ApiResponse<SearchResponseDto>.Ok(result));
    }

    [HttpGet("projects")]
    public async Task<ActionResult<ApiResponse<SearchResponseDto>>> SearchProjects(
        [FromQuery] SearchFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _searchService.SearchProjectsAsync(filter, cancellationToken);
        return Ok(ApiResponse<SearchResponseDto>.Ok(result));
    }

    [HttpGet("tasks")]
    public async Task<ActionResult<ApiResponse<SearchResponseDto>>> SearchTasks(
        [FromQuery] SearchFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _searchService.SearchTasksAsync(filter, cancellationToken);
        return Ok(ApiResponse<SearchResponseDto>.Ok(result));
    }

    [HttpGet("bugs")]
    public async Task<ActionResult<ApiResponse<SearchResponseDto>>> SearchBugs(
        [FromQuery] SearchFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _searchService.SearchBugsAsync(filter, cancellationToken);
        return Ok(ApiResponse<SearchResponseDto>.Ok(result));
    }
}
