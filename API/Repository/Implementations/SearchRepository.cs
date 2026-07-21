using JiraTrack.Models.DTOs.Search;
using JiraTrack.Models.Entities;
using JiraTrack.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JiraTrack.Repository.Implementations;

public class SearchRepository : ISearchRepository
{
    private readonly ApplicationDbContext _context;

    public SearchRepository(ApplicationDbContext context) => _context = context;

    public async Task<SearchResponseDto> SearchAllAsync(
        string term, List<int> projectIds, bool isAdmin, SearchFilterRequest filter, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Trim().Length < 2)
            return EmptyResponse(filter);

        var normalized = term.Trim();
        var results = new List<ScoredResult>();

        if (isAdmin)
        {
            var projectEntities = await _context.Projects
                .Where(p => !p.IsArchived && MatchesProject(p, normalized))
                .Take(50)
                .ToListAsync(cancellationToken);
            results.AddRange(projectEntities.Select(p =>
            {
                var dto = MapProject(p, normalized);
                return new ScoredResult(dto, ScoreProject(dto, normalized));
            }));
        }
        else if (projectIds.Count > 0)
        {
            var projectEntities = await _context.Projects
                .Where(p => projectIds.Contains(p.Id) && !p.IsArchived && MatchesProject(p, normalized))
                .Take(50)
                .ToListAsync(cancellationToken);
            results.AddRange(projectEntities.Select(p =>
            {
                var dto = MapProject(p, normalized);
                return new ScoredResult(dto, ScoreProject(dto, normalized));
            }));
        }

        if (projectIds.Count > 0 || isAdmin)
        {
            var taskQuery = _context.TaskItems
                .Include(t => t.Project)
                .Where(t => t.ParentTaskId == null && MatchesTask(t, normalized));

            if (!isAdmin)
                taskQuery = taskQuery.Where(t => projectIds.Contains(t.ProjectId));

            var taskEntities = await taskQuery.Take(100).ToListAsync(cancellationToken);
            results.AddRange(taskEntities.Select(t =>
            {
                var dto = MapTask(t, normalized);
                return new ScoredResult(dto, ScoreTask(dto, normalized));
            }));

            var bugQuery = _context.Bugs
                .Include(b => b.Project)
                .Where(b => MatchesBug(b, normalized));

            if (!isAdmin)
                bugQuery = bugQuery.Where(b => projectIds.Contains(b.ProjectId));

            var bugEntities = await bugQuery.Take(100).ToListAsync(cancellationToken);
            results.AddRange(bugEntities.Select(b =>
            {
                var dto = MapBug(b, normalized);
                return new ScoredResult(dto, ScoreBug(dto, normalized));
            }));
        }

        var ordered = results
            .OrderByDescending(r => r.Score)
            .ThenBy(r => r.Item.Type)
            .Select(r => r.Item)
            .ToList();

        return Paginate(ordered, filter);
    }

    public async Task<SearchResponseDto> SearchProjectsAsync(
        string term, List<int> projectIds, bool isAdmin, SearchFilterRequest filter, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(term) || term.Trim().Length < 2)
            return EmptyResponse(filter);

        var normalized = term.Trim();
        var query = _context.Projects.Where(p => !p.IsArchived && MatchesProject(p, normalized));

        if (!isAdmin)
            query = query.Where(p => projectIds.Contains(p.Id));

        var total = await query.CountAsync(cancellationToken);
        var entities = await query
            .OrderBy(p => p.Name)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var items = entities.Select(p => MapProject(p, normalized)).ToList();
        return BuildResponse(items, total, filter);
    }

    public async Task<SearchResponseDto> SearchTasksAsync(
        string term, List<int> projectIds, SearchFilterRequest filter, CancellationToken cancellationToken = default)
    {
        if (projectIds.Count == 0 || string.IsNullOrWhiteSpace(term) || term.Trim().Length < 2)
            return EmptyResponse(filter);

        var normalized = term.Trim();
        var query = _context.TaskItems
            .Include(t => t.Project)
            .Where(t => projectIds.Contains(t.ProjectId) && t.ParentTaskId == null && MatchesTask(t, normalized));

        var total = await query.CountAsync(cancellationToken);
        var entities = await query
            .OrderByDescending(t => t.UpdatedDate ?? t.CreatedDate)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var items = entities.Select(t => MapTask(t, normalized)).ToList();
        return BuildResponse(items, total, filter);
    }

    public async Task<SearchResponseDto> SearchBugsAsync(
        string term, List<int> projectIds, SearchFilterRequest filter, CancellationToken cancellationToken = default)
    {
        if (projectIds.Count == 0 || string.IsNullOrWhiteSpace(term) || term.Trim().Length < 2)
            return EmptyResponse(filter);

        var normalized = term.Trim();
        var query = _context.Bugs
            .Include(b => b.Project)
            .Where(b => projectIds.Contains(b.ProjectId) && MatchesBug(b, normalized));

        var total = await query.CountAsync(cancellationToken);
        var entities = await query
            .OrderByDescending(b => b.UpdatedDate ?? b.CreatedDate)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var items = entities.Select(b => MapBug(b, normalized)).ToList();
        return BuildResponse(items, total, filter);
    }

    private static SearchResultDto MapProject(Project p, string term) => new()
    {
        Type = "Project",
        Id = p.Id,
        ProjectId = p.Id,
        Key = p.Key,
        Title = p.Name,
        Subtitle = p.Description,
        Status = p.IsArchived ? "Archived" : "Active",
        MatchedField = GetProjectMatchedField(p, term)
    };

    private static SearchResultDto MapProject(Project p) => MapProject(p, string.Empty);

    private static SearchResultDto MapTask(TaskItem t, string term) => new()
    {
        Type = "Task",
        Id = t.Id,
        ProjectId = t.ProjectId,
        Key = t.TaskKey,
        Title = t.Title,
        Subtitle = t.Project.Name,
        Status = t.Status.ToString(),
        MatchedField = GetTaskMatchedField(t, term)
    };

    private static SearchResultDto MapBug(Bug b, string term) => new()
    {
        Type = "Bug",
        Id = b.Id,
        ProjectId = b.ProjectId,
        Key = b.BugKey,
        Title = b.Title,
        Subtitle = b.Project.Name,
        Status = b.Status.ToString(),
        MatchedField = GetBugMatchedField(b, term)
    };

    private static bool MatchesProject(Project p, string term) =>
        p.Key.Contains(term) ||
        p.Name.Contains(term) ||
        (p.Description != null && p.Description.Contains(term));

    private static bool MatchesTask(TaskItem t, string term) =>
        t.TaskKey.Contains(term) ||
        t.Title.Contains(term) ||
        (t.Description != null && t.Description.Contains(term));

    private static bool MatchesBug(Bug b, string term) =>
        b.BugKey.Contains(term) ||
        b.Title.Contains(term) ||
        (b.Description != null && b.Description.Contains(term));

    private static string GetProjectMatchedField(Project p, string term)
    {
        if (p.Key.Contains(term, StringComparison.OrdinalIgnoreCase)) return "Key";
        if (p.Name.Contains(term, StringComparison.OrdinalIgnoreCase)) return "Name";
        return "Description";
    }

    private static string GetTaskMatchedField(TaskItem t, string term)
    {
        if (t.TaskKey.Contains(term, StringComparison.OrdinalIgnoreCase)) return "Key";
        if (t.Title.Contains(term, StringComparison.OrdinalIgnoreCase)) return "Title";
        return "Description";
    }

    private static string GetBugMatchedField(Bug b, string term)
    {
        if (b.BugKey.Contains(term, StringComparison.OrdinalIgnoreCase)) return "Key";
        if (b.Title.Contains(term, StringComparison.OrdinalIgnoreCase)) return "Title";
        return "Description";
    }

    private static int ScoreProject(SearchResultDto item, string term)
    {
        if (item.Key.Equals(term, StringComparison.OrdinalIgnoreCase)) return 100;
        if (item.Key.StartsWith(term, StringComparison.OrdinalIgnoreCase)) return 80;
        if (item.Title.Contains(term, StringComparison.OrdinalIgnoreCase)) return 60;
        return 40;
    }

    private static int ScoreTask(SearchResultDto item, string term) => ScoreProject(item, term);
    private static int ScoreBug(SearchResultDto item, string term) => ScoreProject(item, term);

    private static SearchResponseDto Paginate(List<SearchResultDto> items, SearchFilterRequest filter)
    {
        var total = items.Count;
        var pageItems = items
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToList();
        return BuildResponse(pageItems, total, filter);
    }

    private static SearchResponseDto BuildResponse(List<SearchResultDto> items, int total, SearchFilterRequest filter) =>
        new()
        {
            Items = items,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalCount = total
        };

    private static SearchResponseDto EmptyResponse(SearchFilterRequest filter) =>
        new() { PageNumber = filter.PageNumber, PageSize = filter.PageSize };

    private sealed record ScoredResult(SearchResultDto Item, int Score);
}
