using JiraTrack.Models.Enums;

namespace JiraTrack.Models.Entities;

public class Sprint : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public SprintStatus Status { get; set; } = SprintStatus.Planning;

    public ICollection<TaskItem> Tasks { get; set; } = [];
}
