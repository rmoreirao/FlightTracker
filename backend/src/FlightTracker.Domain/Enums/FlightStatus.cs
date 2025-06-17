namespace FlightTracker.Domain.Enums;

/// <summary>
/// Flight status enumeration
/// </summary>
public enum FlightStatus
{
    Scheduled = 1,
    Delayed = 2,
    Boarding = 3,
    InFlight = 4,
    Landed = 5,
    Cancelled = 6,
    Diverted = 7
}
