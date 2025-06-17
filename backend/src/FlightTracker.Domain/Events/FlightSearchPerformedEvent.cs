using FlightTracker.Domain.Entities;

namespace FlightTracker.Domain.Events;

/// <summary>
/// Event raised when a flight search is performed
/// </summary>
public class FlightSearchPerformedEvent : DomainEvent
{
    public FlightQuery Query { get; }
    public int ResultCount { get; }
    public TimeSpan SearchDuration { get; }

    public FlightSearchPerformedEvent(FlightQuery query, int resultCount, TimeSpan searchDuration)
    {
        Query = query ?? throw new ArgumentNullException(nameof(query));
        ResultCount = resultCount;
        SearchDuration = searchDuration;
    }
}
