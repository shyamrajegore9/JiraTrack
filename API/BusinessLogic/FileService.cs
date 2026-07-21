using AutoMapper;
using System.Security.Claims;
using JiraTrack.Models.DTOs.Auth;
using JiraTrack.Models.DTOs.Files;
using JiraTrack.Models.Entities;
using JiraTrack.Models.Enums;
using JiraTrack.Repository.Interfaces;
using JiraTrack.Settings;
using Microsoft.Extensions.Options;

namespace JiraTrack.BusinessLogic;

public class FileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _storage;
    private readonly IVirusScanService _virusScan;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly FileStorageSettings _settings;
    private readonly IMapper _mapper;

    public FileService(
        IUnitOfWork unitOfWork,
        IFileStorageService storage,
        IVirusScanService virusScan,
        IHttpContextAccessor httpContextAccessor,
        IOptions<FileStorageSettings> settings,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
        _virusScan = virusScan;
        _httpContextAccessor = httpContextAccessor;
        _settings = settings.Value;
        _mapper = mapper;
    }

    public async Task<List<AttachmentDto>> GetAttachmentsAsync(
        string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        var normalizedType = NormalizeEntityType(entityType);
        await EnsureEntityReadAccessAsync(normalizedType, entityId, cancellationToken);
        return await _unitOfWork.Attachments.GetByEntityAsync(normalizedType, entityId, cancellationToken);
    }

    public async Task<AttachmentDto> UploadAsync(
        IFormFile file, string entityType, int entityId, CancellationToken cancellationToken = default)
    {
        var normalizedType = NormalizeEntityType(entityType);
        await EnsureEntityWriteAccessAsync(normalizedType, entityId, cancellationToken);

        var userId = GetCurrentUserId();
        var sanitizedName = FileValidationHelper.SanitizeFileName(file.FileName);

        if (!FileValidationHelper.TryGetFileType(sanitizedName, out var fileType))
            throw new BusinessException("File extension is not allowed.");

        ValidateFileSize(fileType, file.Length);
        FileValidationHelper.ValidateContentType(fileType, file.ContentType);

        await using var stream = file.OpenReadStream();
        await FileValidationHelper.ValidateMagicBytesAsync(fileType, stream, cancellationToken);
        await _virusScan.ScanAsync(stream, sanitizedName, cancellationToken);
        stream.Position = 0;

        var (storedFileName, relativePath) = await _storage.SaveAsync(stream, sanitizedName, fileType, cancellationToken);

        var attachment = new Attachment
        {
            EntityType = normalizedType,
            EntityId = entityId,
            FileName = sanitizedName,
            StoredFileName = relativePath,
            ContentType = file.ContentType.Split(';')[0].Trim(),
            FileSize = file.Length,
            FileType = fileType,
            UploadedBy = userId,
            UploadedDate = DateTime.UtcNow
        };

        await _unitOfWork.Attachments.AddAsync(attachment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var uploader = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        var uploaderName = uploader != null ? $"{uploader.FirstName} {uploader.LastName}" : "User";
        return MapToDto(attachment, uploaderName);
    }

    public async Task<UserProfileDto> UploadProfilePictureAsync(
        IFormFile file, CancellationToken cancellationToken = default)
    {
        if (!FileValidationHelper.TryGetFileType(file.FileName, out var fileType) || fileType != FileType.Image)
            throw new BusinessException("Profile picture must be a JPG, PNG, GIF, or WebP image.");

        var userId = GetCurrentUserId();
        return await UploadProfilePictureForUserAsync(file, userId, cancellationToken);
    }

    public async Task<FileDownloadResult> DownloadAsync(int id, CancellationToken cancellationToken = default)
    {
        var attachment = await _unitOfWork.Attachments.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Attachment not found.");

        await EnsureEntityReadAccessAsync(attachment.EntityType, attachment.EntityId, cancellationToken);

        var stream = await _storage.OpenReadAsync(attachment.StoredFileName, cancellationToken);
        return new FileDownloadResult
        {
            Stream = stream,
            ContentType = attachment.ContentType,
            FileName = attachment.FileName
        };
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var attachment = await _unitOfWork.Attachments.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Attachment not found.");

        var userId = GetCurrentUserId();
        if (attachment.UploadedBy != userId && !IsAdmin())
            throw new ForbiddenBusinessException("You can only delete files you uploaded.");

        await EnsureEntityWriteAccessAsync(attachment.EntityType, attachment.EntityId, cancellationToken);

        await _storage.DeleteAsync(attachment.StoredFileName, cancellationToken);
        _unitOfWork.Attachments.Delete(attachment);

        if (attachment.EntityType == AttachmentEntityTypes.User)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(attachment.EntityId, cancellationToken);
            if (user?.ProfilePictureUrl?.Contains($"/files/{attachment.Id}") == true)
            {
                user.ProfilePictureUrl = null;
                user.UpdatedBy = userId;
                user.UpdatedDate = DateTime.UtcNow;
                _unitOfWork.Users.Update(user);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<UserProfileDto> UploadProfilePictureForUserAsync(
        IFormFile file, int userId, CancellationToken cancellationToken)
    {
        ValidateFileSize(FileType.Image, file.Length);
        FileValidationHelper.ValidateContentType(FileType.Image, file.ContentType);

        var sanitizedName = FileValidationHelper.SanitizeFileName(file.FileName);

        await using var stream = file.OpenReadStream();
        await FileValidationHelper.ValidateMagicBytesAsync(FileType.Image, stream, cancellationToken);
        await _virusScan.ScanAsync(stream, sanitizedName, cancellationToken);
        stream.Position = 0;

        var (_, relativePath) = await _storage.SaveAsync(stream, sanitizedName, FileType.Image, cancellationToken);

        var attachment = new Attachment
        {
            EntityType = AttachmentEntityTypes.User,
            EntityId = userId,
            FileName = sanitizedName,
            StoredFileName = relativePath,
            ContentType = file.ContentType.Split(';')[0].Trim(),
            FileSize = file.Length,
            FileType = FileType.Image,
            UploadedBy = userId,
            UploadedDate = DateTime.UtcNow
        };

        await _unitOfWork.Attachments.AddAsync(attachment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var user = await _unitOfWork.Users.GetByIdWithRolesAsync(userId, cancellationToken)
            ?? throw new NotFoundException("User not found.");

        user.ProfilePictureUrl = $"/api/v1/files/{attachment.Id}";
        user.UpdatedBy = userId;
        user.UpdatedDate = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<UserProfileDto>(user);
    }

    private void ValidateFileSize(FileType fileType, long size)
    {
        var max = fileType switch
        {
            FileType.Image => _settings.MaxImageSizeBytes,
            FileType.Document => _settings.MaxDocumentSizeBytes,
            FileType.Video => _settings.MaxVideoSizeBytes,
            _ => _settings.MaxDocumentSizeBytes
        };

        if (size <= 0)
            throw new BusinessException("File is empty.");

        if (size > max)
            throw new BusinessException($"File exceeds the maximum size of {max / (1024 * 1024)} MB for {fileType} files.");
    }

    private async Task EnsureEntityReadAccessAsync(string entityType, int entityId, CancellationToken cancellationToken)
    {
        if (IsAdmin()) return;

        switch (entityType)
        {
            case AttachmentEntityTypes.User:
                return;

            case AttachmentEntityTypes.Task:
                var task = await _unitOfWork.Tasks.GetByIdAsync(entityId, cancellationToken)
                    ?? throw new NotFoundException("Task not found.");
                if (!await _unitOfWork.Projects.IsMemberAsync(task.ProjectId, GetCurrentUserId(), cancellationToken))
                    throw new ForbiddenBusinessException("You are not a member of this project.");
                return;

            case AttachmentEntityTypes.Bug:
                var bug = await _unitOfWork.Bugs.GetByIdAsync(entityId, cancellationToken)
                    ?? throw new NotFoundException("Bug not found.");
                if (!await _unitOfWork.Projects.IsMemberAsync(bug.ProjectId, GetCurrentUserId(), cancellationToken))
                    throw new ForbiddenBusinessException("You are not a member of this project.");
                return;

            case AttachmentEntityTypes.Comment:
                var comment = await _unitOfWork.Comments.GetByIdAsync(entityId, cancellationToken)
                    ?? throw new NotFoundException("Comment not found.");
                await EnsureEntityReadAccessAsync(comment.EntityType, comment.EntityId, cancellationToken);
                return;

            default:
                throw new BusinessException("Unsupported entity type.");
        }
    }

    private async Task EnsureEntityWriteAccessAsync(string entityType, int entityId, CancellationToken cancellationToken)
    {
        if (IsViewer())
            throw new ForbiddenBusinessException("Viewers have read-only access.");

        if (entityType == AttachmentEntityTypes.User && entityId == GetCurrentUserId())
            return;

        await EnsureEntityReadAccessAsync(entityType, entityId, cancellationToken);
    }

    private static AttachmentDto MapToDto(Attachment attachment, string uploadedByName) => new()
    {
        Id = attachment.Id,
        EntityType = attachment.EntityType,
        EntityId = attachment.EntityId,
        FileName = attachment.FileName,
        ContentType = attachment.ContentType,
        FileSize = attachment.FileSize,
        FileType = attachment.FileType,
        UploadedBy = attachment.UploadedBy,
        UploadedByName = uploadedByName,
        UploadedDate = attachment.UploadedDate
    };

    private static string NormalizeEntityType(string entityType) =>
        AttachmentEntityTypes.All.First(t => t.Equals(entityType, StringComparison.OrdinalIgnoreCase));

    private int GetCurrentUserId() =>
        int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.Admin);
    private bool IsViewer() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.Viewer)
        && !IsAdmin() && !_httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.ProjectManager);
}
