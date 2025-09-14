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
}
