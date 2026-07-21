using Asp.Versioning;
using JiraTrack.BusinessLogic;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Kanban;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraTrack.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects/{projectId:int}/kanban")]
[Authorize]
public class KanbanController : ControllerBase
{
    private readonly KanbanService _kanbanService;

    public KanbanController(KanbanService kanbanService) => _kanbanService = kanbanService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<KanbanBoardDto>>> GetBoard(
        int projectId, [FromQuery] KanbanFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _kanbanService.GetBoardAsync(projectId, filter, cancellationToken);
        return Ok(ApiResponse<KanbanBoardDto>.Ok(result));
    }

    [HttpPatch("move")]
    public async Task<ActionResult<ApiResponse<KanbanCardDto>>> MoveCard(
        int projectId, [FromBody] MoveKanbanCardRequest request, CancellationToken cancellationToken)
    {
        var result = await _kanbanService.MoveCardAsync(projectId, request, cancellationToken);
        return Ok(ApiResponse<KanbanCardDto>.Ok(result, "Card moved"));
    }

    [HttpPatch("reorder")]
    public async Task<ActionResult<ApiResponse>> ReorderCards(
        int projectId, [FromBody] ReorderKanbanCardsRequest request, CancellationToken cancellationToken)
    {
        await _kanbanService.ReorderCardsAsync(projectId, request, cancellationToken);
        return Ok(ApiResponse.Ok("Cards reordered"));
    }
}
