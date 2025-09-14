namespace FlightTracker.Domain.Enums;

/// <summary>
/// Direction classification for an itinerary leg. Supports future multi-city itineraries.
/// </summary>
public enum LegDirection
{
    Outbound = 0,
    Return = 1,
    Intermediate = 2 // For future multi-city/open-jaw legs
}
