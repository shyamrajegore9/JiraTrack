using JiraTrack.Models.DTOs.Dashboard;
using JiraTrack.Models.DTOs.Reports;
using JiraTrack.Models.Entities;
using JiraTrack.Models.Enums;
using JiraTrack.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JiraTrack.Repository.Implementations;

public class ReportRepository : IReportRepository
{
    private readonly ApplicationDbContext _context;

    public ReportRepository(ApplicationDbContext context) => _context = context;

    public async Task<DeveloperReportDto> GetDeveloperReportAsync(
        int userId, List<int> projectIds, ReportFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstAsync(u => u.Id == userId, cancellationToken);
        var scopedProjects = ScopeProjects(projectIds, filter.ProjectId);

        var tasksQuery = _context.TaskItems
            .Include(t => t.Project)
            .Where(t => scopedProjects.Contains(t.ProjectId)
                && t.AssigneeId == userId
                && t.Status == TaskItemStatus.Done
                && t.ParentTaskId == null);

        tasksQuery = ApplyTaskDateFilter(tasksQuery, filter);

        var completedTasks = await tasksQuery
            .OrderByDescending(t => t.UpdatedDate ?? t.CreatedDate)
            .Select(t => new DeveloperTaskRowDto
            {
                TaskKey = t.TaskKey,
                Title = t.Title,
                ProjectName = t.Project.Name,
                CompletedDate = t.UpdatedDate ?? t.CreatedDate
            })
            .ToListAsync(cancellationToken);

        var timeLogsQuery = _context.Set<TimeLog>()
            .Include(t => t.Task).ThenInclude(t => t.Project)
            .Where(t => t.UserId == userId && scopedProjects.Contains(t.Task.ProjectId));

        timeLogsQuery = ApplyTimeLogDateFilter(timeLogsQuery, filter);

        var timeLogs = await timeLogsQuery
            .OrderByDescending(t => t.WorkDate)
            .Select(t => new ReportTimeLogRowDto
            {
                TaskKey = t.Task.TaskKey,
                ProjectName = t.Task.Project.Name,
                Hours = t.Hours,
                WorkDate = t.WorkDate,
                Description = t.Description
            })
            .ToListAsync(cancellationToken);

        var bugsQuery = _context.Bugs
            .Where(b => scopedProjects.Contains(b.ProjectId)
                && b.DeveloperId == userId
                && (b.Status == BugItemStatus.Fixed || b.Status == BugItemStatus.Closed));

        bugsQuery = ApplyBugDateFilter(bugsQuery, filter);
        var bugsFixed = await bugsQuery.CountAsync(cancellationToken);

        return new DeveloperReportDto
        {
            UserId = userId,
            UserName = user.UserName,
            FullName = $"{user.FirstName} {user.LastName}".Trim(),
            TasksCompleted = completedTasks.Count,
            HoursLogged = timeLogs.Sum(t => t.Hours),
            BugsFixed = bugsFixed,
            CompletedTasks = completedTasks,
            TimeLogs = timeLogs
        };
    }

    public async Task<BugReportDto> GetBugReportAsync(
        List<int> projectIds, ReportFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var scopedProjects = ScopeProjects(projectIds, filter.ProjectId);
        var query = _context.Bugs.Where(b => scopedProjects.Contains(b.ProjectId));
        query = ApplyBugDateFilter(query, filter);

        var total = await query.CountAsync(cancellationToken);

        var bySeverity = await query
            .GroupBy(b => b.Severity)
            .Select(g => new ChartSliceDto { Label = g.Key.ToString(), Value = g.Count() })
            .ToListAsync(cancellationToken);

        var byStatus = await query
            .GroupBy(b => b.Status)
            .Select(g => new ChartSliceDto { Label = g.Key.ToString(), Value = g.Count() })
            .ToListAsync(cancellationToken);

        var byEnvironment = await query
            .Where(b => b.Environment != null && b.Environment != "")
            .GroupBy(b => b.Environment!)
            .Select(g => new ChartSliceDto { Label = g.Key, Value = g.Count() })
            .ToListAsync(cancellationToken);

        return new BugReportDto
        {
            TotalBugs = total,
            BySeverity = bySeverity.OrderBy(s => s.Label).ToList(),
            ByStatus = byStatus.OrderBy(s => s.Label).ToList(),
            ByEnvironment = byEnvironment.OrderByDescending(s => s.Value).ToList()
        };
    }

    public async Task<ProjectReportDto> GetProjectReportAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var project = await _context.Projects
            .Include(p => p.Members)
            .FirstAsync(p => p.Id == projectId, cancellationToken);

        var tasks = _context.TaskItems.Where(t => t.ProjectId == projectId && t.ParentTaskId == null);
        var totalTasks = await tasks.CountAsync(cancellationToken);
        var completedTasks = await tasks.CountAsync(t => t.Status == TaskItemStatus.Done, cancellationToken);
        var openTasks = totalTasks - completedTasks;

        var bugs = _context.Bugs.Where(b => b.ProjectId == projectId);
        var totalBugs = await bugs.CountAsync(cancellationToken);
        var openBugs = await bugs.CountAsync(b => b.Status != BugItemStatus.Closed, cancellationToken);

        var tasksByStatus = await tasks
            .GroupBy(t => t.Status)
            .Select(g => new ChartSliceDto { Label = g.Key.ToString(), Value = g.Count() })
            .ToListAsync(cancellationToken);

        var bugsBySeverity = await bugs
            .Where(b => b.Status != BugItemStatus.Closed)
            .GroupBy(b => b.Severity)
            .Select(g => new ChartSliceDto { Label = g.Key.ToString(), Value = g.Count() })
            .ToListAsync(cancellationToken);

        var activeSprint = await _context.Sprints
            .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.Status == SprintStatus.Active, cancellationToken);

        return new ProjectReportDto
        {
            ProjectId = projectId,
            ProjectKey = project.Key,
            ProjectName = project.Name,
            TotalTasks = totalTasks,
            OpenTasks = openTasks,
            CompletedTasks = completedTasks,
            TaskCompletionRate = totalTasks == 0 ? 0 : Math.Round((decimal)completedTasks / totalTasks * 100, 1),
            TotalBugs = totalBugs,
            OpenBugs = openBugs,
            MemberCount = project.Members.Count,
            HasActiveSprint = activeSprint != null,
            ActiveSprintName = activeSprint?.Name,
            TasksByStatus = tasksByStatus,
            BugsBySeverity = bugsBySeverity
        };
    }

    public async Task<TimeTrackingReportDto> GetTimeTrackingReportAsync(
        List<int> projectIds, ReportFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var scopedProjects = ScopeProjects(projectIds, filter.ProjectId);

        var query = _context.Set<TimeLog>()
            .Include(t => t.User)
            .Include(t => t.Task).ThenInclude(t => t.Project)
            .Where(t => scopedProjects.Contains(t.Task.ProjectId));

        if (filter.UserId.HasValue)
            query = query.Where(t => t.UserId == filter.UserId);

        query = ApplyTimeLogDateFilter(query, filter);

        var rows = await query
            .OrderByDescending(t => t.WorkDate)
            .Select(t => new TimeTrackingRowDto
            {
                UserId = t.UserId,
                UserName = t.User.FirstName + " " + t.User.LastName,
                ProjectId = t.Task.ProjectId,
                ProjectName = t.Task.Project.Name,
                TaskKey = t.Task.TaskKey,
                Hours = t.Hours,
                WorkDate = t.WorkDate,
                Description = t.Description
            })
            .ToListAsync(cancellationToken);

        return new TimeTrackingReportDto
        {
            TotalHours = rows.Sum(r => r.Hours),
            Rows = rows
        };
    }

    private static List<int> ScopeProjects(List<int> projectIds, int? filterProjectId)
    {
        if (filterProjectId.HasValue)
            return projectIds.Contains(filterProjectId.Value) ? [filterProjectId.Value] : [];
        return projectIds;
    }

    private static IQueryable<TaskItem> ApplyTaskDateFilter(IQueryable<TaskItem> query, ReportFilterRequest filter)
    {
        if (filter.StartDate.HasValue)
            query = query.Where(t => (t.UpdatedDate ?? t.CreatedDate) >= filter.StartDate.Value);
        if (filter.EndDate.HasValue)
            query = query.Where(t => (t.UpdatedDate ?? t.CreatedDate) <= filter.EndDate.Value.AddDays(1));
        return query;
    }

    private static IQueryable<Bug> ApplyBugDateFilter(IQueryable<Bug> query, ReportFilterRequest filter)
    {
        if (filter.StartDate.HasValue)
            query = query.Where(b => (b.UpdatedDate ?? b.CreatedDate) >= filter.StartDate.Value);
        if (filter.EndDate.HasValue)
            query = query.Where(b => (b.UpdatedDate ?? b.CreatedDate) <= filter.EndDate.Value.AddDays(1));
        return query;
    }

    private static IQueryable<TimeLog> ApplyTimeLogDateFilter(IQueryable<TimeLog> query, ReportFilterRequest filter)
    {
        if (filter.StartDate.HasValue)
            query = query.Where(t => t.WorkDate >= filter.StartDate.Value.Date);
        if (filter.EndDate.HasValue)
            query = query.Where(t => t.WorkDate <= filter.EndDate.Value.Date);
        return query;
    }
}
