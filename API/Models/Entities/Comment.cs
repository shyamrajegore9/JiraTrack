namespace JiraTrack.Models.Entities;

public class Comment : BaseEntity
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Content { get; set; } = string.Empty;

    public ICollection<Comment> Replies { get; set; } = [];
    public ICollection<CommentMention> Mentions { get; set; } = [];
    public ICollection<CommentReaction> Reactions { get; set; } = [];
}

public class CommentMention
{
    public int CommentId { get; set; }
    public Comment Comment { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}

public class CommentReaction
{
    public int CommentId { get; set; }
    public Comment Comment { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Emoji { get; set; } = string.Empty;
}
