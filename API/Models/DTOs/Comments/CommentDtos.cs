namespace JiraTrack.Models.DTOs.Comments;

public class CommentDto
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int? ParentCommentId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsEdited { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public List<MentionDto> Mentions { get; set; } = [];
    public List<CommentReactionGroupDto> Reactions { get; set; } = [];
    public List<CommentDto> Replies { get; set; } = [];
}

public class MentionDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

public class CommentReactionGroupDto
{
    public string Emoji { get; set; } = string.Empty;
    public int Count { get; set; }
    public bool ReactedByMe { get; set; }
    public List<string> UserNames { get; set; } = [];
}

public class CreateCommentRequest
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class UpdateCommentRequest
{
    public string Content { get; set; } = string.Empty;
}

public class AddReactionRequest
{
    public string Emoji { get; set; } = string.Empty;
}

public class CommentFilterRequest
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
}
