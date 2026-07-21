using JiraTrack.Models.DTOs.Dashboard;
using JiraTrack.Models.Entities;
using JiraTrack.Models.Enums;
using JiraTrack.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JiraTrack.Repository.Implementations;

public class DashboardRepository : IDashboardRepository
{
    private readonly ApplicationDbContext _context;

    public DashboardRepository(ApplicationDbContext context) => _context = context;

    public async Task<List<int>> GetAccessibleProjectIdsAsync(int userId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        if (isAdmin)
        {
            return await _context.Projects
                .Where(p => !p.IsArchived)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);
        }

        return await _context.ProjectMembers
            .Where(m => m.UserId == userId && !m.Project.IsArchived)
            .Select(m => m.ProjectId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(List<int> projectIds, CancellationToken cancellationToken = default)
    {
        if (projectIds.Count == 0)
            return new DashboardSummaryDto();

        var tasks = _context.TaskItems.Where(t => projectIds.Contains(t.ProjectId) && t.ParentTaskId == null);
        var bugs = _context.Bugs.Where(b => projectIds.Contains(b.ProjectId));

        var openTasks = await tasks.CountAsync(t => t.Status != TaskItemStatus.Done, cancellationToken);
        var openBugs = await bugs.CountAsync(b => b.Status != BugItemStatus.Closed, cancellationToken);

        var weekAgo = DateTime.UtcNow.AddDays(-7);
        var completedThisWeek = await tasks.CountAsync(
            t => t.Status == TaskItemStatus.Done && (t.UpdatedDate ?? t.CreatedDate) >= weekAgo,
            cancellationToken);

        var sprintProgress = await CalculateSprintProgressAsync(projectIds, cancellationToken);

        var tasksByStatus = await tasks
            .GroupBy(t => t.Status)
            .Select(g => new ChartSliceDto { Label = g.Key.ToString(), Value = g.Count() })
            .ToListAsync(cancellationToken);

        var bugsBySeverity = await bugs
            .GroupBy(b => b.Severity)
            .Select(g => new ChartSliceDto { Label = g.Key.ToString(), Value = g.Count() })
            .ToListAsync(cancellationToken);

        return new DashboardSummaryDto
        {
            OpenTasks = openTasks,
            OpenBugs = openBugs,
            SprintProgressPercent = sprintProgress,
            CompletedThisWeek = completedThisWeek,
            ActiveProjects = projectIds.Count,
            TasksByStatus = OrderTaskStatusSlices(tasksByStatus),
            BugsBySeverity = OrderSeveritySlices(bugsBySeverity)
        };
    }

    public async Task<List<MyTaskWidgetDto>> GetMyTasksAsync(
        int userId, List<int> projectIds, int limit, CancellationToken cancellationToken = default)
    {
        if (projectIds.Count == 0) return [];

        return await _context.TaskItems
            .Where(t => projectIds.Contains(t.ProjectId)
                && t.AssigneeId == userId
                && t.Status != TaskItemStatus.Done
                && t.ParentTaskId == null)
            .OrderBy(t => t.DueDate ?? DateTime.MaxValue)
            .ThenByDescending(t => t.Priority)
            .Take(limit)
            .Select(t => new MyTaskWidgetDto
            {
                Id = t.Id,
                ProjectId = t.ProjectId,
                TaskKey = t.TaskKey,
                Title = t.Title,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                DueDate = t.DueDate
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ActivityItemDto>> GetRecentActivityAsync(
        List<int> projectIds, int limit, CancellationToken cancellationToken = default)
    {
        if (projectIds.Count == 0) return [];

        var fetchLimit = limit * 2;

        var taskActivities = await _context.TaskItems
            .Where(t => projectIds.Contains(t.ProjectId) && t.ParentTaskId == null)
            .OrderByDescending(t => t.UpdatedDate ?? t.CreatedDate)
            .Take(fetchLimit)
            .Select(t => new ActivityItemDto
            {
                Type = t.UpdatedDate.HasValue && t.UpdatedDate > t.CreatedDate ? "TaskUpdated" : "TaskCreated",
                Message = t.UpdatedDate.HasValue && t.UpdatedDate > t.CreatedDate
                    ? $"Task {t.TaskKey} updated ({t.Status})"
                    : $"Task {t.TaskKey} created",
                ProjectId = t.ProjectId,
                EntityType = "Task",
                EntityId = t.Id,
                CreatedDate = t.UpdatedDate ?? t.CreatedDate
            })
            .ToListAsync(cancellationToken);

        var bugActivities = await _context.Bugs
            .Where(b => projectIds.Contains(b.ProjectId))
            .OrderByDescending(b => b.UpdatedDate ?? b.CreatedDate)
            .Take(fetchLimit)
            .Select(b => new ActivityItemDto
            {
                Type = b.UpdatedDate.HasValue && b.UpdatedDate > b.CreatedDate ? "BugUpdated" : "BugCreated",
                Message = b.UpdatedDate.HasValue && b.UpdatedDate > b.CreatedDate
                    ? $"Bug {b.BugKey} updated ({b.Status})"
                    : $"Bug {b.BugKey} reported",
                ProjectId = b.ProjectId,
                EntityType = "Bug",
                EntityId = b.Id,
                CreatedDate = b.UpdatedDate ?? b.CreatedDate
            })
            .ToListAsync(cancellationToken);

        var commentItems = new List<ActivityItemDto>();
        var comments = await _context.Comments
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedDate)
            .Take(fetchLimit * 3)
            .ToListAsync(cancellationToken);

        foreach (var comment in comments)
        {
            var projectId = await ResolveCommentProjectIdAsync(comment.EntityType, comment.EntityId, cancellationToken);
            if (!projectId.HasValue || !projectIds.Contains(projectId.Value)) continue;

            commentItems.Add(new ActivityItemDto
            {
                Type = "CommentAdded",
                Message = $"{comment.User.FirstName} {comment.User.LastName} commented on {comment.EntityType.ToLower()}",
                ProjectId = projectId,
                EntityType = comment.EntityType,
                EntityId = comment.EntityId,
                CreatedDate = comment.CreatedDate
            });
            if (commentItems.Count >= fetchLimit) break;
        }

        var sprintActivities = await _context.Sprints
            .Where(s => projectIds.Contains(s.ProjectId) && s.Status != SprintStatus.Planning)
            .OrderByDescending(s => s.UpdatedDate ?? s.CreatedDate)
            .Take(fetchLimit)
            .Select(s => new ActivityItemDto
            {
                Type = s.Status == SprintStatus.Active ? "SprintStarted" : "SprintClosed",
                Message = s.Status == SprintStatus.Active
                    ? $"Sprint \"{s.Name}\" started"
                    : $"Sprint \"{s.Name}\" closed",
                ProjectId = s.ProjectId,
                CreatedDate = s.UpdatedDate ?? s.CreatedDate
            })
            .ToListAsync(cancellationToken);

        return taskActivities
            .Concat(bugActivities)
            .Concat(commentItems)
            .Concat(sprintActivities)
            .OrderByDescending(a => a.CreatedDate)
            .Take(limit)
            .ToList();
    }

    public async Task<BugSummaryDto> GetBugSummaryAsync(List<int> projectIds, CancellationToken cancellationToken = default)
    {
        if (projectIds.Count == 0)
            return new BugSummaryDto();

        var bugs = _context.Bugs.Where(b => projectIds.Contains(b.ProjectId));
        var total = await bugs.CountAsync(cancellationToken);
        var closed = await bugs.CountAsync(b => b.Status == BugItemStatus.Closed, cancellationToken);
        var open = total - closed;

        var bySeverity = await bugs
            .Where(b => b.Status != BugItemStatus.Closed)
            .GroupBy(b => b.Severity)
            .Select(g => new ChartSliceDto { Label = g.Key.ToString(), Value = g.Count() })
            .ToListAsync(cancellationToken);

        return new BugSummaryDto
        {
            Open = open,
            Closed = closed,
            Total = total,
            BySeverity = OrderSeveritySlices(bySeverity)
        };
    }

    private async Task<int> CalculateSprintProgressAsync(List<int> projectIds, CancellationToken cancellationToken)
    {
        var activeSprints = await _context.Sprints
            .Include(s => s.Tasks)
            .Where(s => projectIds.Contains(s.ProjectId) && s.Status == SprintStatus.Active)
            .ToListAsync(cancellationToken);

        if (activeSprints.Count == 0) return 0;

        var progressValues = activeSprints.Select(s =>
        {
            var tasks = s.Tasks.Where(t => !t.IsDeleted && t.ParentTaskId == null).ToList();
            if (tasks.Count == 0) return 0.0;
            return tasks.Count(t => t.Status == TaskItemStatus.Done) * 100.0 / tasks.Count;
        }).ToList();

        return (int)Math.Round(progressValues.Average());
    }

    private async Task<int?> ResolveCommentProjectIdAsync(string entityType, int entityId, CancellationToken cancellationToken)
    {
        if (entityType == CommentEntityTypes.Task)
        {
            return await _context.TaskItems
                .Where(t => t.Id == entityId)
                .Select(t => (int?)t.ProjectId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (entityType == CommentEntityTypes.Bug)
        {
            return await _context.Bugs
                .Where(b => b.Id == entityId)
                .Select(b => (int?)b.ProjectId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return null;
    }

    private static List<ChartSliceDto> OrderTaskStatusSlices(List<ChartSliceDto> slices)
    {
        var order = new[] { "Todo", "InProgress", "CodeReview", "Testing", "Done" };
        return slices.OrderBy(s => Array.IndexOf(order, s.Label)).ToList();
    }

    private static List<ChartSliceDto> OrderSeveritySlices(List<ChartSliceDto> slices)
    {
        var order = new[] { "Critical", "High", "Medium", "Low" };
        return slices.OrderBy(s => Array.IndexOf(order, s.Label)).ToList();
    }
}
