namespace JiraTrack.Models.Entities;

public class Project : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int LeadUserId { get; set; }
    public User LeadUser { get; set; } = null!;
    public bool IsArchived { get; set; }
    public int TaskCounter { get; set; }
    public int BugCounter { get; set; }

    public ICollection<ProjectMember> Members { get; set; } = [];
    public ICollection<TaskItem> Tasks { get; set; } = [];
    public ICollection<Bug> Bugs { get; set; } = [];
    public ICollection<Sprint> Sprints { get; set; } = [];
    public ICollection<Label> Labels { get; set; } = [];
}

public class ProjectMember : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string ProjectRole { get; set; } = string.Empty;
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
}
