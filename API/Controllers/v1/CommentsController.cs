using Asp.Versioning;
using JiraTrack.BusinessLogic;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraTrack.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/comments")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly CommentService _commentService;

    public CommentsController(CommentService commentService) => _commentService = commentService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CommentDto>>>> GetComments(
        [FromQuery] CommentFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _commentService.GetCommentsAsync(filter, cancellationToken);
        return Ok(ApiResponse<List<CommentDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CommentDto>>> CreateComment(
        [FromBody] CreateCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await _commentService.CreateCommentAsync(request, cancellationToken);
        return Ok(ApiResponse<CommentDto>.Ok(result, "Comment added"));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<CommentDto>>> UpdateComment(
        int id, [FromBody] UpdateCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await _commentService.UpdateCommentAsync(id, request, cancellationToken);
        return Ok(ApiResponse<CommentDto>.Ok(result, "Comment updated"));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> DeleteComment(int id, CancellationToken cancellationToken)
    {
        await _commentService.DeleteCommentAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Comment deleted"));
    }

    [HttpPost("{id:int}/reactions")]
    public async Task<ActionResult<ApiResponse<CommentDto>>> AddReaction(
        int id, [FromBody] AddReactionRequest request, CancellationToken cancellationToken)
    {
        var result = await _commentService.AddReactionAsync(id, request, cancellationToken);
        return Ok(ApiResponse<CommentDto>.Ok(result, "Reaction added"));
    }

    [HttpDelete("{id:int}/reactions/{emoji}")]
    public async Task<ActionResult<ApiResponse<CommentDto>>> RemoveReaction(
        int id, string emoji, CancellationToken cancellationToken)
    {
        var decodedEmoji = Uri.UnescapeDataString(emoji);
        var result = await _commentService.RemoveReactionAsync(id, decodedEmoji, cancellationToken);
        return Ok(ApiResponse<CommentDto>.Ok(result, "Reaction removed"));
    }
}
