using System.Collections.Generic;using FlightTracker.Api.Application.DTOs;

namespace FlightTracker.Api.Application.DTOs;

public class SearchItinerariesResult
{
    public required IEnumerable<ItineraryDto> Items { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Returned { get; init; }
    public string SortBy { get; init; } = string.Empty;
    public string SortOrder { get; init; } = string.Empty;
    public bool RoundTripRequested { get; init; }
}
