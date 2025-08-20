using FlightTracker.Domain.Enums;

namespace FlightTracker.Domain.ValueObjects;

/// <summary>
/// Value object for flight search options including sorting and pagination
/// </summary>
public sealed record FlightSearchOptions
{
    /// <summary>
    /// Field to sort by
    /// </summary>
    public FlightSortBy SortBy { get; init; } = FlightSortBy.DepartureTime;
    
    /// <summary>
    /// Sort order (ascending or descending)
    /// </summary>
    public SortOrder SortOrder { get; init; } = SortOrder.Ascending;
    
    /// <summary>
    /// Page number for pagination (1-based)
    /// </summary>
    public int Page { get; init; } = 1;
    
    /// <summary>
    /// Number of results per page
    /// </summary>
    public int PageSize { get; init; } = 20;
    
    /// <summary>
    /// Maximum number of results to return (0 = no limit)
    /// </summary>
    public int MaxResults { get; init; } = 0;

    /// <summary>
    /// Creates new flight search options with default values
    /// </summary>
    public static FlightSearchOptions Default => new();
    
    /// <summary>
    /// Creates new flight search options with specified sorting
    /// </summary>
    public static FlightSearchOptions WithSorting(FlightSortBy sortBy, SortOrder sortOrder = SortOrder.Ascending)
        => new() { SortBy = sortBy, SortOrder = sortOrder };
    
    /// <summary>
    /// Creates new flight search options with pagination
    /// </summary>
    public static FlightSearchOptions WithPagination(int page, int pageSize)
        => new() { Page = Math.Max(1, page), PageSize = Math.Max(1, Math.Min(100, pageSize)) };
    
    /// <summary>
    /// Creates new flight search options with sorting and pagination
    /// </summary>
    public static FlightSearchOptions Create(
        FlightSortBy sortBy = FlightSortBy.DepartureTime,
        SortOrder sortOrder = SortOrder.Ascending,
        int page = 1,
        int pageSize = 20,
        int maxResults = 0)
    {
        return new FlightSearchOptions
        {
            SortBy = sortBy,
            SortOrder = sortOrder,
            Page = Math.Max(1, page),
            PageSize = Math.Max(1, Math.Min(100, pageSize)),
            MaxResults = Math.Max(0, maxResults)
        };
    }
    
    /// <summary>
    /// Calculates the number of items to skip for pagination
    /// </summary>
    public int Skip => (Page - 1) * PageSize;
    
    /// <summary>
    /// Validates the search options
    /// </summary>
    public bool IsValid()
    {
        return Page >= 1 && 
               PageSize >= 1 && PageSize <= 100 && 
               MaxResults >= 0;
    }
}
