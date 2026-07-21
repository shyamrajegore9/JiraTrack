namespace JiraTrack.Models.DTOs.Sprints;

public class SprintListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int TaskCount { get; set; }
    public decimal TotalStoryPoints { get; set; }
    public decimal CompletedStoryPoints { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class SprintDetailDto : SprintListDto
{
    public List<SprintBacklogTaskDto> Backlog { get; set; } = [];
}

public class SprintBacklogTaskDto
{
    public int Id { get; set; }
    public string TaskKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? AssigneeName { get; set; }
    public decimal? StoryPoints { get; set; }
}

public class CreateSprintRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class UpdateSprintRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class AddTaskToSprintBacklogRequest
{
    public int TaskId { get; set; }
}

public class SprintVelocityDto
{
    public int SprintId { get; set; }
    public string SprintName { get; set; } = string.Empty;
    public decimal CompletedStoryPoints { get; set; }
    public decimal TotalStoryPoints { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public decimal CompletionRate { get; set; }
}

public class BurndownDto
{
    public int SprintId { get; set; }
    public decimal TotalStoryPoints { get; set; }
    public List<BurndownPointDto> Points { get; set; } = [];
}

public class BurndownPointDto
{
    public DateTime Date { get; set; }
    public decimal IdealRemaining { get; set; }
    public decimal ActualRemaining { get; set; }
}
