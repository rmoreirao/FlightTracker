using FlightTracker.Domain.Enums;

namespace FlightTracker.Domain.ValueObjects;

/// <summary>
/// Options controlling itinerary search, pairing and sorting.
/// </summary>
public sealed record ItinerarySearchOptions
{
    public int MaxOutboundFlights { get; init; } = 40;
    public int MaxReturnFlights { get; init; } = 40;
    public int MaxCombinations { get; init; } = 400;
    public TimeSpan? MinStay { get; init; } = null; // For round-trip constraint
    public TimeSpan? MaxStay { get; init; } = null;
    public FlightSortBy SortBy { get; init; } = FlightSortBy.Price;
    public SortOrder SortOrder { get; init; } = SortOrder.Ascending;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public bool AllowMixedCabin { get; init; } = false;

    public int Skip => (Page - 1) * PageSize;

    public static ItinerarySearchOptions Default => new();

    /// <summary>
    /// Factory with normalization & guard clauses used by API layer.
    /// </summary>
    public static ItinerarySearchOptions Create(
        FlightSortBy sortBy,
        SortOrder sortOrder,
        int page,
        int pageSize,
        int maxOutbound,
        int maxReturn,
        int maxCombos,
        bool allowMixedCabin = false,
        TimeSpan? minStay = null,
        TimeSpan? maxStay = null)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 1 : pageSize > 200 ? 200 : pageSize; // Hard upper bound
        maxOutbound = Math.Max(1, maxOutbound);
        maxReturn = Math.Max(1, maxReturn);
        maxCombos = Math.Max(1, maxCombos);
        if (minStay.HasValue && minStay.Value < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(minStay));
        if (maxStay.HasValue && maxStay.Value < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(maxStay));
        if (minStay.HasValue && maxStay.HasValue && minStay > maxStay)
            throw new ArgumentException("MinStay cannot be greater than MaxStay");

        return new ItinerarySearchOptions
        {
            SortBy = sortBy,
            SortOrder = sortOrder,
            Page = page,
            PageSize = pageSize,
            MaxOutboundFlights = maxOutbound,
            MaxReturnFlights = maxReturn,
            MaxCombinations = maxCombos,
            AllowMixedCabin = allowMixedCabin,
            MinStay = minStay,
            MaxStay = maxStay
        };
    }
}
