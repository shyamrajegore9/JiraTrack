using JiraTrack.Models.DTOs.Search;

namespace JiraTrack.Repository.Interfaces;

public interface ISearchRepository
{
    Task<SearchResponseDto> SearchAllAsync(string term, List<int> projectIds, bool isAdmin, SearchFilterRequest filter, CancellationToken cancellationToken = default);
    Task<SearchResponseDto> SearchProjectsAsync(string term, List<int> projectIds, bool isAdmin, SearchFilterRequest filter, CancellationToken cancellationToken = default);
    Task<SearchResponseDto> SearchTasksAsync(string term, List<int> projectIds, SearchFilterRequest filter, CancellationToken cancellationToken = default);
    Task<SearchResponseDto> SearchBugsAsync(string term, List<int> projectIds, SearchFilterRequest filter, CancellationToken cancellationToken = default);
}
