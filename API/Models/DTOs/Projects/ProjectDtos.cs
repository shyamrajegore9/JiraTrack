namespace JiraTrack.Models.DTOs.Projects;

public class ProjectListDto
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string LeadName { get; set; } = string.Empty;
    public int LeadUserId { get; set; }
    public bool IsArchived { get; set; }
    public int MemberCount { get; set; }
    public int TaskCount { get; set; }
    public int BugCount { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class ProjectDetailDto : ProjectListDto
{
    public string? LeadEmail { get; set; }
}

public class CreateProjectRequest
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int LeadUserId { get; set; }
}

public class UpdateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int LeadUserId { get; set; }
}

public class ProjectFilterRequest
{
    private int _pageSize = 20;

    public int PageNumber { get; set; } = 1;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 100 ? 100 : value < 1 ? 20 : value;
    }
    public string? SearchTerm { get; set; }
    public bool? IsArchived { get; set; }
    public string SortBy { get; set; } = "CreatedDate";
    public string SortDirection { get; set; } = "desc";
}

public class ProjectDashboardDto
{
    public int ProjectId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int OpenTasks { get; set; }
    public int TotalBugs { get; set; }
    public int OpenBugs { get; set; }
    public int MemberCount { get; set; }
    public bool IsArchived { get; set; }
    public bool HasActiveSprint { get; set; }
    public string? ActiveSprintName { get; set; }
}

public class ProjectMemberDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ProjectRole { get; set; } = string.Empty;
    public DateTime JoinedDate { get; set; }
}

public class AddProjectMemberRequest
{
    public int UserId { get; set; }
    public string ProjectRole { get; set; } = string.Empty;
}

public class UpdateProjectSettingsRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public static class ProjectRoles
{
    public const string ProjectManager = "ProjectManager";
    public const string Developer = "Developer";
    public const string QA = "QA";
    public const string Viewer = "Viewer";

    public static readonly string[] All = [ProjectManager, Developer, QA, Viewer];
}
