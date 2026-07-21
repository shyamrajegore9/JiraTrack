using JiraTrack.Models.Entities;
using JiraTrack.Models.Enums;
using JiraTrack.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JiraTrack.Repository.Implementations;

public class SprintRepository : GenericRepository<Sprint>, ISprintRepository
{
    public SprintRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<Sprint>> GetByProjectAsync(int projectId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(s => s.Tasks.Where(t => !t.IsDeleted && t.ParentTaskId == null))
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync(cancellationToken);

    public async Task<Sprint?> GetByIdWithTasksAsync(int projectId, int sprintId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(s => s.Tasks.Where(t => !t.IsDeleted && t.ParentTaskId == null))
            .ThenInclude(t => t.Assignee)
            .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.Id == sprintId, cancellationToken);

    public async Task<Sprint?> GetActiveSprintAsync(int projectId, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(s => s.ProjectId == projectId && s.Status == SprintStatus.Active, cancellationToken);

    public async Task<bool> HasActiveSprintAsync(int projectId, int? excludeSprintId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(s => s.ProjectId == projectId && s.Status == SprintStatus.Active);
        if (excludeSprintId.HasValue)
            query = query.Where(s => s.Id != excludeSprintId.Value);
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<List<TaskItem>> GetBacklogTasksAsync(int sprintId, CancellationToken cancellationToken = default) =>
        await Context.Set<TaskItem>()
            .Include(t => t.Assignee)
            .Where(t => t.SprintId == sprintId && t.ParentTaskId == null)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.CreatedDate)
            .ToListAsync(cancellationToken);

    public async Task<List<TaskItem>> GetSprintTasksAsync(int sprintId, CancellationToken cancellationToken = default) =>
        await Context.Set<TaskItem>()
            .Where(t => t.SprintId == sprintId && t.ParentTaskId == null)
            .ToListAsync(cancellationToken);
}
