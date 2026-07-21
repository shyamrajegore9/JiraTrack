using JiraTrack.Models.Enums;

namespace JiraTrack.Models.Entities;

public class TaskItem : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public int? SprintId { get; set; }
    public Sprint? Sprint { get; set; }
    public string TaskKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AcceptanceCriteria { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public int? AssigneeId { get; set; }
    public User? Assignee { get; set; }
    public int ReporterId { get; set; }
    public User Reporter { get; set; } = null!;
    public decimal? StoryPoints { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int SortOrder { get; set; }
    public int? ParentTaskId { get; set; }
    public TaskItem? ParentTask { get; set; }

    public ICollection<TaskItem> Subtasks { get; set; } = [];
    public ICollection<ChecklistItem> ChecklistItems { get; set; } = [];
    public ICollection<TaskLabel> TaskLabels { get; set; } = [];
    public ICollection<TimeLog> TimeLogs { get; set; } = [];
}

public class Label : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#757575";

    public ICollection<TaskLabel> TaskLabels { get; set; } = [];
}

public class TaskLabel
{
    public int TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;
    public int LabelId { get; set; }
    public Label Label { get; set; } = null!;
}

public class ChecklistItem : BaseEntity
{
    public int TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int SortOrder { get; set; }
}

public class TimeLog : BaseEntity
{
    public int TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public decimal Hours { get; set; }
    public DateTime WorkDate { get; set; }
    public string? Description { get; set; }
}
