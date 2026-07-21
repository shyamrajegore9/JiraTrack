namespace JiraTrack.Models.DTOs.Bugs;

public class BugListDto
{
    public int Id { get; set; }
    public string BugKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? DeveloperName { get; set; }
    public int? DeveloperId { get; set; }
    public string? TesterName { get; set; }
    public int? TesterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public string? Environment { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class BugDetailDto : BugListDto
{
    public string? Description { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string? StepsToReproduce { get; set; }
    public string? ExpectedResult { get; set; }
    public string? ActualResult { get; set; }
}

public class CreateBugRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Severity { get; set; } = "Medium";
    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Open";
    public string? Environment { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string? StepsToReproduce { get; set; }
    public string? ExpectedResult { get; set; }
    public string? ActualResult { get; set; }
    public int? DeveloperId { get; set; }
    public int? TesterId { get; set; }
}

public class UpdateBugRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Severity { get; set; } = "Medium";
    public string Priority { get; set; } = "Medium";
    public string? Environment { get; set; }
    public string? Browser { get; set; }
    public string? OperatingSystem { get; set; }
    public string? StepsToReproduce { get; set; }
    public string? ExpectedResult { get; set; }
    public string? ActualResult { get; set; }
    public int? DeveloperId { get; set; }
    public int? TesterId { get; set; }
}

public class UpdateBugStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class AssignDeveloperRequest
{
    public int? DeveloperId { get; set; }
}

public class AssignTesterRequest
{
    public int? TesterId { get; set; }
}

public class BugFilterRequest
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
    public string? Severity { get; set; }
    public string? Priority { get; set; }
    public int? DeveloperId { get; set; }
    public int? TesterId { get; set; }
    public string SortBy { get; set; } = "CreatedDate";
    public string SortDirection { get; set; } = "desc";
}
