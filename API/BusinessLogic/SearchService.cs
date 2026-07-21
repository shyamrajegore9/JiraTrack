using System.Security.Claims;
using JiraTrack.Models.DTOs.Search;
using JiraTrack.Repository.Interfaces;
using JiraTrack.Settings;

namespace JiraTrack.BusinessLogic;

public class SearchService
{
    private const int MinSearchLength = 2;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SearchService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<SearchResponseDto> SearchAllAsync(SearchFilterRequest filter, CancellationToken cancellationToken = default)
    {
        ValidateQuery(filter.Q);
        var (projectIds, isAdmin) = await GetSearchScopeAsync(cancellationToken);
        return await _unitOfWork.Search.SearchAllAsync(filter.Q, projectIds, isAdmin, filter, cancellationToken);
    }

    public async Task<SearchResponseDto> SearchProjectsAsync(SearchFilterRequest filter, CancellationToken cancellationToken = default)
    {
        ValidateQuery(filter.Q);
        var (projectIds, isAdmin) = await GetSearchScopeAsync(cancellationToken);
        return await _unitOfWork.Search.SearchProjectsAsync(filter.Q, projectIds, isAdmin, filter, cancellationToken);
    }

    public async Task<SearchResponseDto> SearchTasksAsync(SearchFilterRequest filter, CancellationToken cancellationToken = default)
    {
        ValidateQuery(filter.Q);
        var (projectIds, _) = await GetSearchScopeAsync(cancellationToken);
        return await _unitOfWork.Search.SearchTasksAsync(filter.Q, projectIds, filter, cancellationToken);
    }

    public async Task<SearchResponseDto> SearchBugsAsync(SearchFilterRequest filter, CancellationToken cancellationToken = default)
    {
        ValidateQuery(filter.Q);
        var (projectIds, _) = await GetSearchScopeAsync(cancellationToken);
        return await _unitOfWork.Search.SearchBugsAsync(filter.Q, projectIds, filter, cancellationToken);
    }

    private async Task<(List<int> ProjectIds, bool IsAdmin)> GetSearchScopeAsync(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var isAdmin = IsAdmin();
        var projectIds = await _unitOfWork.Dashboard.GetAccessibleProjectIdsAsync(userId, isAdmin, cancellationToken);
        return (projectIds, isAdmin);
    }

    private static void ValidateQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < MinSearchLength)
            throw new BusinessException($"Search query must be at least {MinSearchLength} characters.", 400);
    }

    private int GetCurrentUserId() =>
        int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin() => _httpContextAccessor.HttpContext!.User.IsInRole(AppRoles.Admin);
}
