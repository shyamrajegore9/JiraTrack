using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Bugs;
using JiraTrack.Models.Entities;
using JiraTrack.Models.Enums;
using JiraTrack.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JiraTrack.Repository.Implementations;

public class BugRepository : GenericRepository<Bug>, IBugRepository
{
    public BugRepository(ApplicationDbContext context) : base(context) { }

    public async Task<PagedResponse<Bug>> GetPagedAsync(int projectId, BugFilterRequest filter, CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(b => b.Developer)
            .Include(b => b.Tester)
            .Include(b => b.Reporter)
            .Where(b => b.ProjectId == projectId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(b => b.Title.Contains(term) || b.BugKey.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<BugItemStatus>(filter.Status, true, out var status))
            query = query.Where(b => b.Status == status);

        if (!string.IsNullOrWhiteSpace(filter.Severity) && Enum.TryParse<BugSeverity>(filter.Severity, true, out var severity))
            query = query.Where(b => b.Severity == severity);

        if (!string.IsNullOrWhiteSpace(filter.Priority) && Enum.TryParse<TaskPriority>(filter.Priority, true, out var priority))
            query = query.Where(b => b.Priority == priority);

        if (filter.DeveloperId.HasValue)
            query = query.Where(b => b.DeveloperId == filter.DeveloperId);

        if (filter.TesterId.HasValue)
            query = query.Where(b => b.TesterId == filter.TesterId);

        query = ApplySorting(query, filter.SortBy, filter.SortDirection);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<Bug>
        {
            Items = items,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<Bug?> GetByIdWithDetailsAsync(int projectId, int bugId, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(b => b.Developer)
            .Include(b => b.Tester)
            .Include(b => b.Reporter)
            .FirstOrDefaultAsync(b => b.ProjectId == projectId && b.Id == bugId, cancellationToken);

    public async Task<(int Total, int Open)> GetBugCountsAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var bugs = DbSet.Where(b => b.ProjectId == projectId);
        var total = await bugs.CountAsync(cancellationToken);
        var open = await bugs.CountAsync(b => b.Status != BugItemStatus.Closed, cancellationToken);
        return (total, open);
    }

    public async Task<string> GenerateBugKeyAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var project = await Context.Set<Project>().FirstAsync(p => p.Id == projectId, cancellationToken);
        project.BugCounter++;
        Context.Set<Project>().Update(project);
        return $"{project.Key}-BUG-{project.BugCounter}";
    }

    private static IQueryable<Bug> ApplySorting(IQueryable<Bug> query, string sortBy, string sortDirection)
    {
        var desc = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
        return sortBy.ToLowerInvariant() switch
        {
            "bugkey" => desc ? query.OrderByDescending(b => b.BugKey) : query.OrderBy(b => b.BugKey),
            "title" => desc ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
            "status" => desc ? query.OrderByDescending(b => b.Status) : query.OrderBy(b => b.Status),
            "severity" => desc ? query.OrderByDescending(b => b.Severity) : query.OrderBy(b => b.Severity),
            "priority" => desc ? query.OrderByDescending(b => b.Priority) : query.OrderBy(b => b.Priority),
            _ => desc ? query.OrderByDescending(b => b.CreatedDate) : query.OrderBy(b => b.CreatedDate)
        };
    }
}
