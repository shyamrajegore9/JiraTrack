using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Projects;
using JiraTrack.Models.Entities;
using JiraTrack.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JiraTrack.Repository.Implementations;

public class ProjectRepository : GenericRepository<Project>, IProjectRepository
{
    public ProjectRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Project?> GetByKeyAsync(string key, CancellationToken cancellationToken = default) =>
        await DbSet.FirstOrDefaultAsync(p => p.Key == key.ToUpperInvariant(), cancellationToken);

    public async Task<Project?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
        await DbSet
            .Include(p => p.LeadUser)
            .Include(p => p.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<PagedResponse<Project>> GetPagedAsync(
        ProjectFilterRequest filter,
        int? userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(p => p.LeadUser)
            .Include(p => p.Members)
            .AsQueryable();

        if (!isAdmin && userId.HasValue)
            query = query.Where(p => p.Members.Any(m => m.UserId == userId.Value));

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(p =>
                p.Key.Contains(term) ||
                p.Name.Contains(term) ||
                (p.Description != null && p.Description.Contains(term)));
        }

        if (filter.IsArchived.HasValue)
            query = query.Where(p => p.IsArchived == filter.IsArchived.Value);

        query = ApplySorting(query, filter.SortBy, filter.SortDirection);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<Project>
        {
            Items = items,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<bool> IsMemberAsync(int projectId, int userId, CancellationToken cancellationToken = default) =>
        await Context.Set<ProjectMember>()
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId && !m.IsDeleted, cancellationToken);

    public async Task<List<ProjectMember>> GetMembersAsync(int projectId, CancellationToken cancellationToken = default) =>
        await Context.Set<ProjectMember>()
            .Include(m => m.User)
            .Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.User.FirstName)
            .ToListAsync(cancellationToken);

    public async Task<ProjectMember?> GetMemberAsync(int projectId, int userId, CancellationToken cancellationToken = default) =>
        await Context.Set<ProjectMember>()
            .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId, cancellationToken);

    public async Task AddMemberAsync(ProjectMember member, CancellationToken cancellationToken = default) =>
        await Context.Set<ProjectMember>().AddAsync(member, cancellationToken);

    public void RemoveMember(ProjectMember member)
    {
        member.IsDeleted = true;
        member.DeletedDate = DateTime.UtcNow;
        Context.Set<ProjectMember>().Update(member);
    }

    private static IQueryable<Project> ApplySorting(IQueryable<Project> query, string sortBy, string sortDirection)
    {
        var desc = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLowerInvariant() switch
        {
            "key" => desc ? query.OrderByDescending(p => p.Key) : query.OrderBy(p => p.Key),
            "name" => desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "isarchived" => desc ? query.OrderByDescending(p => p.IsArchived) : query.OrderBy(p => p.IsArchived),
            _ => desc ? query.OrderByDescending(p => p.CreatedDate) : query.OrderBy(p => p.CreatedDate)
        };
    }
}
