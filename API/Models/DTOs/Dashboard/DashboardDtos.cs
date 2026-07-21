namespace JiraTrack.Models.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int OpenTasks { get; set; }
    public int OpenBugs { get; set; }
    public int SprintProgressPercent { get; set; }
    public int CompletedThisWeek { get; set; }
    public int ActiveProjects { get; set; }
    public List<ChartSliceDto> TasksByStatus { get; set; } = [];
    public List<ChartSliceDto> BugsBySeverity { get; set; } = [];
}

public class ChartSliceDto
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}

public class MyTaskWidgetDto
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string TaskKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
}

public class ActivityItemDto
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? ProjectId { get; set; }
    public string? EntityType { get; set; }
    public int? EntityId { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class BugSummaryDto
{
    public int Open { get; set; }
    public int Closed { get; set; }
    public int Total { get; set; }
    public List<ChartSliceDto> BySeverity { get; set; } = [];
}

public class ActivityFilterRequest
{
    private int _limit = 20;

    public int Limit
    {
        get => _limit;
        set => _limit = value > 50 ? 50 : value < 1 ? 20 : value;
    }
}

public class MyTasksFilterRequest
{
    private int _limit = 10;

    public int Limit
    {
        get => _limit;
        set => _limit = value > 50 ? 50 : value < 1 ? 10 : value;
    }
}
