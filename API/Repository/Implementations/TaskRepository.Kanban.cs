using JiraTrack.Models.DTOs.Kanban;
using JiraTrack.Models.Entities;
using JiraTrack.Models.Enums;
using JiraTrack.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JiraTrack.Repository.Implementations;

public partial class TaskRepository
{
    public async Task<List<TaskItem>> GetKanbanTasksAsync(int projectId, KanbanFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(t => t.Assignee)
            .Include(t => t.TaskLabels)
            .ThenInclude(tl => tl.Label)
            .Where(t => t.ProjectId == projectId && t.ParentTaskId == null)
            .AsQueryable();

        if (filter.AssigneeId.HasValue)
            query = query.Where(t => t.AssigneeId == filter.AssigneeId);

        if (filter.SprintId.HasValue)
            query = query.Where(t => t.SprintId == filter.SprintId);

        if (filter.LabelId.HasValue)
            query = query.Where(t => t.TaskLabels.Any(tl => tl.LabelId == filter.LabelId));

        return await query
            .OrderBy(t => t.Status)
            .ThenBy(t => t.SortOrder)
            .ThenBy(t => t.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateSortOrdersAsync(int projectId, TaskItemStatus status, IList<int> taskIds, CancellationToken cancellationToken = default)
    {
        var tasks = await DbSet
            .Where(t => t.ProjectId == projectId && t.Status == status && taskIds.Contains(t.Id))
            .ToListAsync(cancellationToken);

        for (var i = 0; i < taskIds.Count; i++)
        {
            var task = tasks.FirstOrDefault(t => t.Id == taskIds[i]);
            if (task != null)
                task.SortOrder = i;
        }
    }
}
