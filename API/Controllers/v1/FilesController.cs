using Asp.Versioning;
using JiraTrack.BusinessLogic;
using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Auth;
using JiraTrack.Models.DTOs.Files;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JiraTrack.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/files")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly FileService _fileService;

    public FilesController(FileService fileService) => _fileService = fileService;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AttachmentDto>>>> GetAttachments(
        [FromQuery] string entityType,
        [FromQuery] int entityId,
        CancellationToken cancellationToken)
    {
        var result = await _fileService.GetAttachmentsAsync(entityType, entityId, cancellationToken);
        return Ok(ApiResponse<List<AttachmentDto>>.Ok(result));
    }

    [HttpPost("upload")]
    [RequestSizeLimit(105_906_176)]
    public async Task<ActionResult<ApiResponse<AttachmentDto>>> Upload(
        IFormFile file,
        [FromForm] string entityType,
        [FromForm] int entityId,
        CancellationToken cancellationToken)
    {
        var result = await _fileService.UploadAsync(file, entityType, entityId, cancellationToken);
        return Ok(ApiResponse<AttachmentDto>.Ok(result));
    }

    [HttpPost("profile-picture")]
    [RequestSizeLimit(10_485_760)]
    public async Task<ActionResult<ApiResponse<UserProfileDto>>> UploadProfilePicture(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var result = await _fileService.UploadProfilePictureAsync(file, cancellationToken);
        return Ok(ApiResponse<UserProfileDto>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        var result = await _fileService.DownloadAsync(id, cancellationToken);
        return File(result.Stream, result.ContentType, result.FileName);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        await _fileService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(null!));
    }
}
