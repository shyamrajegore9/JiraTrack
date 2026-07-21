using JiraTrack.Models.DTOs.Dashboard;
using JiraTrack.Models.DTOs.Sprints;

namespace JiraTrack.Models.DTOs.Reports;

public class ReportFilterRequest
{
    public int? ProjectId { get; set; }
    public int? UserId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class DeveloperReportDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int TasksCompleted { get; set; }
    public decimal HoursLogged { get; set; }
    public int BugsFixed { get; set; }
    public List<DeveloperTaskRowDto> CompletedTasks { get; set; } = [];
    public List<ReportTimeLogRowDto> TimeLogs { get; set; } = [];
}

public class DeveloperTaskRowDto
{
    public string TaskKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public DateTime CompletedDate { get; set; }
}

public class ReportTimeLogRowDto
{
    public string TaskKey { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public DateTime WorkDate { get; set; }
    public string? Description { get; set; }
}

public class BugReportDto
{
    public int TotalBugs { get; set; }
    public List<ChartSliceDto> BySeverity { get; set; } = [];
    public List<ChartSliceDto> ByStatus { get; set; } = [];
    public List<ChartSliceDto> ByEnvironment { get; set; } = [];
}

public class SprintReportDto
{
    public int SprintId { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string SprintName { get; set; } = string.Empty;
    public string SprintStatus { get; set; } = string.Empty;
    public SprintVelocityDto Velocity { get; set; } = new();
    public BurndownDto Burndown { get; set; } = new();
}

public class ProjectReportDto
{
    public int ProjectId { get; set; }
    public string ProjectKey { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int OpenTasks { get; set; }
    public int CompletedTasks { get; set; }
    public decimal TaskCompletionRate { get; set; }
    public int TotalBugs { get; set; }
    public int OpenBugs { get; set; }
    public int MemberCount { get; set; }
    public bool HasActiveSprint { get; set; }
    public string? ActiveSprintName { get; set; }
    public List<ChartSliceDto> TasksByStatus { get; set; } = [];
    public List<ChartSliceDto> BugsBySeverity { get; set; } = [];
}

public class TimeTrackingReportDto
{
    public decimal TotalHours { get; set; }
    public List<TimeTrackingRowDto> Rows { get; set; } = [];
}

public class TimeTrackingRowDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string TaskKey { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public DateTime WorkDate { get; set; }
    public string? Description { get; set; }
}
