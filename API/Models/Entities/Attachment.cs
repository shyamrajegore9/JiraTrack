using JiraTrack.Models.Enums;

namespace JiraTrack.Models.Entities;

public class Attachment
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public FileType FileType { get; set; }
    public int UploadedBy { get; set; }
    public User UploadedByUser { get; set; } = null!;
    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;
}
