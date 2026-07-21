namespace JiraTrack.Models.DTOs.Tasks;

public class TaskListDto
{
    public int Id { get; set; }
    public string TaskKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? AssigneeName { get; set; }
    public int? AssigneeId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public decimal? StoryPoints { get; set; }
    public DateTime? DueDate { get; set; }
    public List<string> Labels { get; set; } = [];
    public bool IsSubtask { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class TaskDetailDto : TaskListDto
{
    public string? Description { get; set; }
    public string? AcceptanceCriteria { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public DateTime? StartDate { get; set; }
    public int? ParentTaskId { get; set; }
    public string? ParentTaskKey { get; set; }
    public List<ChecklistItemDto> Checklist { get; set; } = [];
    public List<TaskListDto> Subtasks { get; set; } = [];
    public List<LabelDto> LabelDetails { get; set; } = [];
    public List<TimeLogDto> TimeLogs { get; set; } = [];
}

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AcceptanceCriteria { get; set; }
    public string Status { get; set; } = "Todo";
    public string Priority { get; set; } = "Medium";
    public int? AssigneeId { get; set; }
    public decimal? StoryPoints { get; set; }
    public decimal? EstimatedHours { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int? ParentTaskId { get; set; }
    public List<int> LabelIds { get; set; } = [];
}

public class UpdateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AcceptanceCriteria { get; set; }
    public string Priority { get; set; } = "Medium";
    public int? AssigneeId { get; set; }
    public decimal? StoryPoints { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
}

public class UpdateTaskStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class AssignTaskRequest
{
    public int? AssigneeId { get; set; }
}

public class TaskFilterRequest
{
    private int _pageSize = 20;

    public int PageNumber { get; set; } = 1;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 100 ? 100 : value < 1 ? 20 : value;
    }
    public string? SearchTerm { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public int? AssigneeId { get; set; }
    public int? LabelId { get; set; }
    public bool? ParentOnly { get; set; } = true;
    public string SortBy { get; set; } = "CreatedDate";
    public string SortDirection { get; set; } = "desc";
}

public class ChecklistItemDto
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int SortOrder { get; set; }
}

public class CreateChecklistItemRequest
{
    public string Text { get; set; } = string.Empty;
}

public class UpdateChecklistItemRequest
{
    public string Text { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}

public class SetTaskLabelsRequest
{
    public List<int> LabelIds { get; set; } = [];
}

public class LabelDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#757575";
}

public class CreateLabelRequest
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#757575";
}

public class CreateTimeLogRequest
{
    public decimal Hours { get; set; }
    public DateTime WorkDate { get; set; }
    public string? Description { get; set; }
}

public class TimeLogDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public DateTime WorkDate { get; set; }
    public string? Description { get; set; }
}
