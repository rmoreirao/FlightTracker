using FlightTracker.Domain.Entities;

namespace FlightTracker.Api.Application.DTOs;

/// <summary>
/// Result of flight search query
/// </summary>
public class SearchFlightsResult
{
    public IReadOnlyList<Flight> Flights { get; }
    public DateTime LastUpdated { get; }
    public int TotalResults { get; }
    public string Currency { get; }
    public TimeSpan SearchDuration { get; }

    public SearchFlightsResult(
        IReadOnlyList<Flight> flights,
        DateTime lastUpdated,
        string currency = "USD",
        TimeSpan searchDuration = default)
    {
        Flights = flights ?? throw new ArgumentNullException(nameof(flights));
        LastUpdated = lastUpdated;
        TotalResults = flights.Count;
        Currency = currency;
        SearchDuration = searchDuration;
    }
}
