namespace FlightTracker.Domain.Enums;

/// <summary>
/// Enumeration of flight sorting options
/// </summary>
public enum FlightSortBy
{
    /// <summary>
    /// Sort by price (default)
    /// </summary>
    Price = 0,
    
    /// <summary>
    /// Sort by flight duration
    /// </summary>
    Duration = 1,
    
    /// <summary>
    /// Sort by number of stops
    /// </summary>
    Stops = 2,
    
    /// <summary>
    /// Sort by departure time
    /// </summary>
    DepartureTime = 3,
    
    /// <summary>
    /// Sort by arrival time
    /// </summary>
    ArrivalTime = 4,
    
    /// <summary>
    /// Sort by airline name
    /// </summary>
    Airline = 5
}
