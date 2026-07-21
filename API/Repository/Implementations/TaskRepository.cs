using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Tasks;
using JiraTrack.Models.Entities;
using JiraTrack.Models.Enums;
using JiraTrack.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JiraTrack.Repository.Implementations;

public partial class TaskRepository : GenericRepository<TaskItem>, ITaskRepository
{
    public TaskRepository(ApplicationDbContext context) : base(context) { }

    public async Task<PagedResponse<TaskItem>> GetPagedAsync(int projectId, TaskFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(t => t.Assignee)
            .Include(t => t.Reporter)
            .Include(t => t.TaskLabels)
            .ThenInclude(tl => tl.Label)
            .Where(t => t.ProjectId == projectId)
            .AsQueryable();

        if (filter.ParentOnly == true)
            query = query.Where(t => t.ParentTaskId == null);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(t => t.Title.Contains(term) || t.TaskKey.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<TaskItemStatus>(filter.Status, true, out var status))
            query = query.Where(t => t.Status == status);

        if (!string.IsNullOrWhiteSpace(filter.Priority) && Enum.TryParse<TaskPriority>(filter.Priority, true, out var priority))
            query = query.Where(t => t.Priority == priority);

        if (filter.AssigneeId.HasValue)
            query = query.Where(t => t.AssigneeId == filter.AssigneeId);

        if (filter.LabelId.HasValue)
            query = query.Where(t => t.TaskLabels.Any(tl => tl.LabelId == filter.LabelId));

        query = ApplySorting(query, filter.SortBy, filter.SortDirection);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<TaskItem>
        {
            Items = items,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TaskItem?> GetByIdWithDetailsAsync(int projectId, int taskId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(t => t.Assignee)
            .Include(t => t.Reporter)
            .Include(t => t.ParentTask)
            .Include(t => t.TaskLabels).ThenInclude(tl => tl.Label)
            .Include(t => t.ChecklistItems.OrderBy(c => c.SortOrder))
            .Include(t => t.Subtasks.Where(s => !s.IsDeleted)).ThenInclude(s => s.Assignee)
            .Include(t => t.TimeLogs).ThenInclude(tl => tl.User)
            .FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Id == taskId, cancellationToken);

    public async Task<List<TaskItem>> GetSubtasksAsync(int parentTaskId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(t => t.Assignee)
            .Include(t => t.Reporter)
            .Include(t => t.TaskLabels).ThenInclude(tl => tl.Label)
            .Where(t => t.ParentTaskId == parentTaskId)
            .OrderBy(t => t.CreatedDate)
            .ToListAsync(cancellationToken);

    public async Task SetLabelsAsync(int taskId, IEnumerable<int> labelIds, CancellationToken cancellationToken = default)
    {
        var existing = await Context.Set<TaskLabel>().Where(tl => tl.TaskId == taskId).ToListAsync(cancellationToken);
        Context.Set<TaskLabel>().RemoveRange(existing);

        foreach (var labelId in labelIds.Distinct())
        {
            await Context.Set<TaskLabel>().AddAsync(new TaskLabel { TaskId = taskId, LabelId = labelId }, cancellationToken);
        }
    }

    public async Task<List<Label>> GetProjectLabelsAsync(int projectId, CancellationToken cancellationToken = default) =>
        await Context.Set<Label>().Where(l => l.ProjectId == projectId).OrderBy(l => l.Name).ToListAsync(cancellationToken);

    public async Task<Label?> GetLabelByIdAsync(int projectId, int labelId, CancellationToken cancellationToken = default) =>
        await Context.Set<Label>().FirstOrDefaultAsync(l => l.ProjectId == projectId && l.Id == labelId, cancellationToken);

    public async Task<Label> AddLabelAsync(Label label, CancellationToken cancellationToken = default)
    {
        await Context.Set<Label>().AddAsync(label, cancellationToken);
        return label;
    }

    public async Task<List<ChecklistItem>> GetChecklistAsync(int taskId, CancellationToken cancellationToken = default) =>
        await Context.Set<ChecklistItem>().Where(c => c.TaskId == taskId).OrderBy(c => c.SortOrder).ToListAsync(cancellationToken);

    public async Task<ChecklistItem?> GetChecklistItemAsync(int taskId, int itemId, CancellationToken cancellationToken = default) =>
        await Context.Set<ChecklistItem>().FirstOrDefaultAsync(c => c.TaskId == taskId && c.Id == itemId, cancellationToken);

    public async Task AddChecklistItemAsync(ChecklistItem item, CancellationToken cancellationToken = default) =>
        await Context.Set<ChecklistItem>().AddAsync(item, cancellationToken);

    public async Task AddTimeLogAsync(TimeLog timeLog, CancellationToken cancellationToken = default) =>
        await Context.Set<TimeLog>().AddAsync(timeLog, cancellationToken);

    public async Task<List<TimeLog>> GetTimeLogsAsync(int taskId, CancellationToken cancellationToken = default) =>
        await Context.Set<TimeLog>()
            .Include(t => t.User)
            .Where(t => t.TaskId == taskId)
            .OrderByDescending(t => t.WorkDate)
            .ToListAsync(cancellationToken);

    public async Task<(int Total, int Open, int Done)> GetTaskCountsAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var tasks = DbSet.Where(t => t.ProjectId == projectId && t.ParentTaskId == null);
        var total = await tasks.CountAsync(cancellationToken);
        var done = await tasks.CountAsync(t => t.Status == TaskItemStatus.Done, cancellationToken);
        var open = total - done;
        return (total, open, done);
    }

    public async Task<string> GenerateTaskKeyAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var project = await Context.Set<Project>().FirstAsync(p => p.Id == projectId, cancellationToken);
        project.TaskCounter++;
        Context.Set<Project>().Update(project);
        return $"{project.Key}-{project.TaskCounter}";
    }

    private static IQueryable<TaskItem> ApplySorting(IQueryable<TaskItem> query, string sortBy, string sortDirection)
    {
        var desc = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sortBy.ToLowerInvariant() switch
        {
            "taskkey" => desc ? query.OrderByDescending(t => t.TaskKey) : query.OrderBy(t => t.TaskKey),
            "title" => desc ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
            "status" => desc ? query.OrderByDescending(t => t.Status) : query.OrderBy(t => t.Status),
            "priority" => desc ? query.OrderByDescending(t => t.Priority) : query.OrderBy(t => t.Priority),
            "duedate" => desc ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate),
            _ => desc ? query.OrderByDescending(t => t.CreatedDate) : query.OrderBy(t => t.CreatedDate)
        };
    }
}
