using JiraTrack.Models.Enums;

namespace JiraTrack.Models.Entities;

public class Bug : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string BugKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public BugSeverity Severity { get; set; } = BugSeverity.Medium;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public BugItemStatus Status { get; set; } = BugItemStatus.Open;
    public string? Environment { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string? StepsToReproduce { get; set; }
    public string? ExpectedResult { get; set; }
    public string? ActualResult { get; set; }
    public int? DeveloperId { get; set; }
    public User? Developer { get; set; }
    public int? TesterId { get; set; }
    public User? Tester { get; set; }
    public int ReporterId { get; set; }
    public User Reporter { get; set; } = null!;
}
