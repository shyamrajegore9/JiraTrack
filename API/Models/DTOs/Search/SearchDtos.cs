namespace JiraTrack.Models.DTOs.Search;

public class SearchFilterRequest
{
    private int _pageSize = 20;

    public string Q { get; set; } = string.Empty;
    public int PageNumber { get; set; } = 1;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 100 ? 100 : value < 1 ? 20 : value;
    }
}

public class SearchResultDto
{
    public string Type { get; set; } = string.Empty;
    public int Id { get; set; }
    public int? ProjectId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Status { get; set; }
    public string? MatchedField { get; set; }
}

public class SearchResponseDto
{
    public List<SearchResultDto> Items { get; set; } = [];
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}
