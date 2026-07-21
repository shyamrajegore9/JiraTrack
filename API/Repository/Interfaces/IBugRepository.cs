using JiraTrack.Models.Common;
using JiraTrack.Models.DTOs.Bugs;
using JiraTrack.Models.Entities;

namespace JiraTrack.Repository.Interfaces;

public interface IBugRepository : IGenericRepository<Bug>
{
    Task<PagedResponse<Bug>> GetPagedAsync(int projectId, BugFilterRequest filter, CancellationToken cancellationToken = default);
    Task<Bug?> GetByIdWithDetailsAsync(int projectId, int bugId, CancellationToken cancellationToken = default);
    Task<(int Total, int Open)> GetBugCountsAsync(int projectId, CancellationToken cancellationToken = default);
    Task<string> GenerateBugKeyAsync(int projectId, CancellationToken cancellationToken = default);
}
