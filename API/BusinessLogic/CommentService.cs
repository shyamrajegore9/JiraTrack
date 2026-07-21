using System.Security.Claims;
using System.Text.RegularExpressions;
using JiraTrack.Models.DTOs.Comments;
using JiraTrack.Models.Entities;
using JiraTrack.Models.Enums;
using JiraTrack.Repository.Interfaces;
using JiraTrack.Settings;

namespace JiraTrack.BusinessLogic;

public partial class CommentService
{
    private const int MaxReplyDepth = 3;
    private static readonly string[] AllowedEmojis = ["👍", "❤️", "😄", "🎉", "👀", "🚀"];

    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly NotificationService _notificationService;
    private readonly ILogger<CommentService> _logger;

    public CommentService(
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        NotificationService notificationService,
        ILogger<CommentService> logger)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<List<CommentDto>> GetCommentsAsync(CommentFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var entityType = NormalizeEntityType(filter.EntityType);
        var projectId = await ResolveProjectIdAsync(entityType, filter.EntityId, cancellationToken);
        await EnsureProjectAccessAsync(projectId, cancellationToken);

        var comments = await _unitOfWork.Comments.GetByEntityAsync(entityType, filter.EntityId, cancellationToken);
        var currentUserId = GetCurrentUserId();
        var isAdmin = IsAdmin();

        var dtos = comments
            .Where(c => c.ParentCommentId == null)
            .Select(c => MapToDto(c, comments, currentUserId, isAdmin))
            .ToList();

        return dtos;
    }

    public async Task<CommentDto> CreateCommentAsync(CreateCommentRequest request, CancellationToken cancellationToken = default)
    {
        var entityType = NormalizeEntityType(request.EntityType);
        var projectId = await ResolveProjectIdAsync(entityType, request.EntityId, cancellationToken);
        await EnsureProjectWriteAccessAsync(projectId, cancellationToken);

        if (request.ParentCommentId.HasValue)
        {
            var parent = await _unitOfWork.Comments.GetByIdAsync(request.ParentCommentId.Value, cancellationToken)
                ?? throw new NotFoundException("Parent comment not found.");
            if (parent.EntityType != entityType || parent.EntityId != request.EntityId)
                throw new NotFoundException("Parent comment not found.");

            var depth = await GetCommentDepthAsync(request.ParentCommentId.Value, cancellationToken);
            if (depth >= MaxReplyDepth - 1)
                throw new BusinessException($"Maximum reply depth of {MaxReplyDepth} levels reached.", 409);
        }

        var userId = GetCurrentUserId();
        var comment = new Comment
        {
            EntityType = entityType,
            EntityId = request.EntityId,
            ParentCommentId = request.ParentCommentId,
            UserId = userId,
            Content = request.Content.Trim(),
            CreatedBy = userId,
            CreatedDate = DateTime.UtcNow
        };

        await _unitOfWork.Comments.AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var mentionUserIds = await ParseMentionsAsync(request.Content, projectId, cancellationToken);
        if (mentionUserIds.Count > 0)
        {
            await _unitOfWork.Comments.AddMentionsAsync(comment.Id, mentionUserIds, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("Comment {CommentId} added to {EntityType} {EntityId}", comment.Id, entityType, request.EntityId);

        var actorName = await GetActorNameAsync(userId, cancellationToken);
        var entityKey = await GetEntityKeyAsync(entityType, request.EntityId, cancellationToken);
        var stakeholderIds = await GetCommentStakeholderIdsAsync(entityType, request.EntityId, cancellationToken);
        var commentRecipients = stakeholderIds
            .Except([userId])
            .Except(mentionUserIds)
            .ToList();

        if (commentRecipients.Count > 0)
        {
            await _notificationService.NotifyCommentAddedAsync(
                entityType,
                request.EntityId,
                projectId,
                entityKey,
                commentRecipients,
                actorName,
                userId,
                cancellationToken);
        }

        foreach (var mentionedId in mentionUserIds)
        {
            await _notificationService.NotifyMentionAsync(
                mentionedId,
                entityType,
                request.EntityId,
                projectId,
                entityKey,
                actorName,
                userId,
                cancellationToken);
        }

        var saved = await _unitOfWork.Comments.GetByIdWithDetailsAsync(comment.Id, cancellationToken)!;
        return MapToDto(saved!, [], userId, IsAdmin());
    }

    public async Task<CommentDto> UpdateCommentAsync(int id, UpdateCommentRequest request, CancellationToken cancellationToken = default)
    {
        var comment = await _unitOfWork.Comments.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Comment not found.");

        var projectId = await ResolveProjectIdAsync(comment.EntityType, comment.EntityId, cancellationToken);
        await EnsureProjectAccessAsync(projectId, cancellationToken);

        var userId = GetCurrentUserId();
        if (comment.UserId != userId)
            throw new ForbiddenBusinessException("You can only edit your own comments.");

        comment.Content = request.Content.Trim();
        comment.UpdatedBy = userId;
        comment.UpdatedDate = DateTime.UtcNow;
        _unitOfWork.Comments.Update(comment);

        await _unitOfWork.Comments.RemoveMentionsAsync(id, cancellationToken);
        var mentionUserIds = await ParseMentionsAsync(request.Content, projectId, cancellationToken);
        if (mentionUserIds.Count > 0)
            await _unitOfWork.Comments.AddMentionsAsync(id, mentionUserIds, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _unitOfWork.Comments.GetByIdWithDetailsAsync(id, cancellationToken)!;
        return MapToDto(updated!, [], userId, IsAdmin());
    }

    public async Task DeleteCommentAsync(int id, CancellationToken cancellationToken = default)
    {
        var comment = await _unitOfWork.Comments.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Comment not found.");

        var projectId = await ResolveProjectIdAsync(comment.EntityType, comment.EntityId, cancellationToken);
        await EnsureProjectAccessAsync(projectId, cancellationToken);

        var userId = GetCurrentUserId();
        if (comment.UserId != userId && !IsAdmin())
            throw new ForbiddenBusinessException("You do not have permission to delete this comment.");

        _unitOfWork.Comments.SoftDelete(comment, userId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<CommentDto> AddReactionAsync(int id, AddReactionRequest request, CancellationToken cancellationToken = default)
    {
        var comment = await _unitOfWork.Comments.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Comment not found.");

        var projectId = await ResolveProjectIdAsync(comment.EntityType, comment.EntityId, cancellationToken);
        await EnsureProjectWriteAccessAsync(projectId, cancellationToken);

        if (!AllowedEmojis.Contains(request.Emoji))
            throw new BusinessException("Invalid emoji.", 400);

        var userId = GetCurrentUserId();
        var existing = await _unitOfWork.Comments.GetReactionAsync(id, userId, request.Emoji, cancellationToken);
        if (existing == null)
        {
            await _unitOfWork.Comments.AddReactionAsync(new CommentReaction
            {
                CommentId = id,
                UserId = userId,
                Emoji = request.Emoji
            }, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        var updated = await _unitOfWork.Comments.GetByIdWithDetailsAsync(id, cancellationToken)!;
        return MapToDto(updated!, [], userId, IsAdmin());
    }

    public async Task<CommentDto> RemoveReactionAsync(int id, string emoji, CancellationToken cancellationToken = default)
    {
        var comment = await _unitOfWork.Comments.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Comment not found.");

        var projectId = await ResolveProjectIdAsync(comment.EntityType, comment.EntityId, cancellationToken);
        await EnsureProjectAccessAsync(projectId, cancellationToken);

        var userId = GetCurrentUserId();
        var reaction = await _unitOfWork.Comments.GetReactionAsync(id, userId, emoji, cancellationToken)
            ?? throw new NotFoundException("Reaction not found.");

        await _unitOfWork.Comments.RemoveReactionAsync(reaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _unitOfWork.Comments.GetByIdWithDetailsAsync(id, cancellationToken)!;
        return MapToDto(updated!, [], userId, IsAdmin());
    }

    private async Task<int> ResolveProjectIdAsync(string entityType, int entityId, CancellationToken cancellationToken)
    {
        if (entityType == CommentEntityTypes.Task)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(entityId, cancellationToken)
                ?? throw new NotFoundException("Task not found.");
            return task.ProjectId;
        }

        if (entityType == CommentEntityTypes.Bug)
        {
            var bug = await _unitOfWork.Bugs.GetByIdAsync(entityId, cancellationToken)
                ?? throw new NotFoundException("Bug not found.");
            return bug.ProjectId;
        }

        throw new BusinessException("Invalid entity type.", 400);
    }

    private async Task<int> GetCommentDepthAsync(int commentId, CancellationToken cancellationToken)
    {
        var depth = 0;
        var currentId = (int?)commentId;
        while (currentId.HasValue)
        {
            var c = await _unitOfWork.Comments.GetByIdAsync(currentId.Value, cancellationToken);
            if (c?.ParentCommentId == null) return depth;
            depth++;
            currentId = c.ParentCommentId;
        }
        return depth;
    }

    private async Task<List<int>> ParseMentionsAsync(string content, int projectId, CancellationToken cancellationToken)
    {
        var matches = MentionRegex().Matches(content);
        if (matches.Count == 0) return [];

        var members = await _unitOfWork.Projects.GetMembersAsync(projectId, cancellationToken);
        var memberUsers = members.Select(m => m.User).ToList();
        var mentionedIds = new List<int>();

        foreach (Match match in matches)
        {
            var handle = match.Groups[1].Value;
            var user = memberUsers.FirstOrDefault(u =>
                u.UserName.Equals(handle, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Split('@')[0].Equals(handle, StringComparison.OrdinalIgnoreCase));
            if (user != null && !mentionedIds.Contains(user.Id))
                mentionedIds.Add(user.Id);
        }

        return mentionedIds;
    }

    private async Task EnsureProjectAccessAsync(int projectId, CancellationToken cancellationToken)
    {
        if (IsAdmin()) return;
        if (!await _unitOfWork.Projects.IsMemberAsync(projectId, GetCurrentUserId(), cancellationToken))
            throw new ForbiddenBusinessException("You are not a member of this project.");
    }

    private async Task EnsureProjectWriteAccessAsync(int projectId, CancellationToken cancellationToken)
    {
        if (IsViewer()) throw new ForbiddenBusinessException("Viewers have read-only access.");
        await EnsureProjectAccessAsync(projectId, cancellationToken);
    }

    private int GetCurrentUserId() =>
        int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.Admin);
    private bool IsProjectManager() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.ProjectManager);
    private bool IsViewer() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.Viewer)
        && !IsAdmin() && !IsProjectManager();

    private static string NormalizeEntityType(string entityType) =>
        CommentEntityTypes.All.First(t => t.Equals(entityType, StringComparison.OrdinalIgnoreCase));

    private static CommentDto MapToDto(Comment comment, List<Comment> allComments, int currentUserId, bool isAdmin)
    {
        var dto = MapSingle(comment, currentUserId, isAdmin);
        var replies = allComments.Count > 0
            ? allComments.Where(c => c.ParentCommentId == comment.Id)
            : comment.Replies;

        dto.Replies = replies
            .OrderBy(r => r.CreatedDate)
            .Select(r => MapToDto(r, allComments, currentUserId, isAdmin))
            .ToList();

        return dto;
    }

    private static CommentDto MapSingle(Comment comment, int currentUserId, bool isAdmin)
    {
        var isOwner = comment.UserId == currentUserId;
        return new CommentDto
        {
            Id = comment.Id,
            EntityType = comment.EntityType,
            EntityId = comment.EntityId,
            ParentCommentId = comment.ParentCommentId,
            UserId = comment.UserId,
            UserName = comment.User.UserName,
            FullName = $"{comment.User.FirstName} {comment.User.LastName}".Trim(),
            Content = comment.Content,
            CreatedDate = comment.CreatedDate,
            UpdatedDate = comment.UpdatedDate,
            IsEdited = comment.UpdatedDate.HasValue,
            CanEdit = isOwner,
            CanDelete = isOwner || isAdmin,
            Mentions = comment.Mentions.Select(m => new MentionDto
            {
                UserId = m.UserId,
                UserName = m.User.UserName,
                FullName = $"{m.User.FirstName} {m.User.LastName}".Trim()
            }).ToList(),
            Reactions = comment.Reactions
                .GroupBy(r => r.Emoji)
                .Select(g => new CommentReactionGroupDto
                {
                    Emoji = g.Key,
                    Count = g.Count(),
                    ReactedByMe = g.Any(r => r.UserId == currentUserId),
                    UserNames = g.Select(r => r.User.UserName).ToList()
                }).ToList()
        };
    }

    private async Task<string> GetActorNameAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        return user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Someone";
    }

    private async Task<string> GetEntityKeyAsync(string entityType, int entityId, CancellationToken cancellationToken)
    {
        if (entityType == CommentEntityTypes.Task)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(entityId, cancellationToken)
                ?? throw new NotFoundException("Task not found.");
            return task.TaskKey;
        }

        var bug = await _unitOfWork.Bugs.GetByIdAsync(entityId, cancellationToken)
            ?? throw new NotFoundException("Bug not found.");
        return bug.BugKey;
    }

    private async Task<List<int>> GetCommentStakeholderIdsAsync(string entityType, int entityId, CancellationToken cancellationToken)
    {
        if (entityType == CommentEntityTypes.Task)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(entityId, cancellationToken)
                ?? throw new NotFoundException("Task not found.");
            return task.AssigneeId.HasValue ? [task.AssigneeId.Value] : [];
        }

        var bug = await _unitOfWork.Bugs.GetByIdAsync(entityId, cancellationToken)
            ?? throw new NotFoundException("Bug not found.");
        var ids = new List<int>();
        if (bug.DeveloperId.HasValue) ids.Add(bug.DeveloperId.Value);
        if (bug.TesterId.HasValue && bug.TesterId != bug.DeveloperId) ids.Add(bug.TesterId.Value);
        return ids;
    }

    [GeneratedRegex(@"@(\w+)", RegexOptions.Compiled)]
    private static partial Regex MentionRegex();
}
