using FlightTracker.Domain.Enums;
using FlightTracker.Domain.ValueObjects;

namespace FlightTracker.Domain.Entities;

/// <summary>
/// Price snapshot entity representing historical price data for flight searches
/// </summary>
public class PriceSnapshot
{
    public long Id { get; private set; }
    public Guid QueryId { get; private set; }
    public FlightQuery? FlightQuery { get; private set; }
    public string AirlineCode { get; private set; } = string.Empty;
    public Airline? Airline { get; private set; }
    public CabinClass Cabin { get; private set; }
    public Money Price { get; private set; } = null!;
    public string? DeepLink { get; private set; }
    public string? FlightNumber { get; private set; }
    public DateTime? DepartureTime { get; private set; }
    public DateTime? ArrivalTime { get; private set; }
    public int Stops { get; private set; }
    public DateTime CollectedAt { get; private set; }

    public PriceSnapshot(
        Guid queryId,
        string airlineCode,
        CabinClass cabin,
        Money price,
        string? deepLink = null,
        string? flightNumber = null,
        DateTime? departureTime = null,
        DateTime? arrivalTime = null,
        int stops = 0)
    {
        if (queryId == Guid.Empty)
            throw new ArgumentException("Query ID cannot be empty", nameof(queryId));
        
        if (string.IsNullOrWhiteSpace(airlineCode) || airlineCode.Length != 2)
            throw new ArgumentException("Airline code must be exactly 2 characters", nameof(airlineCode));
        
        if (price == null)
            throw new ArgumentNullException(nameof(price));
        
        if (stops < 0)
            throw new ArgumentException("Stops cannot be negative", nameof(stops));
        
        if (departureTime.HasValue && arrivalTime.HasValue && arrivalTime <= departureTime)
            throw new ArgumentException("Arrival time must be after departure time");

        QueryId = queryId;
        AirlineCode = airlineCode.ToUpperInvariant();
        Cabin = cabin;
        Price = price;
        DeepLink = deepLink;
        FlightNumber = flightNumber?.ToUpperInvariant();
        DepartureTime = departureTime;
        ArrivalTime = arrivalTime;
        Stops = stops;
        CollectedAt = DateTime.UtcNow;
    }

    // For EF Core
    private PriceSnapshot() { }

    /// <summary>
    /// Factory method to restore a price snapshot from storage (used by repositories)
    /// </summary>
    public static PriceSnapshot Restore(
        long id,
        Guid queryId,
        string airlineCode,
        CabinClass cabin,
        Money price,
        string? deepLink = null,
        string? flightNumber = null,
        DateTime? departureTime = null,
        DateTime? arrivalTime = null,
        int stops = 0,
        DateTime collectedAt = default)
    {
        var snapshot = new PriceSnapshot
        {
            Id = id,
            QueryId = queryId,
            AirlineCode = airlineCode.ToUpperInvariant(),
            Cabin = cabin,
            Price = price,
            DeepLink = deepLink,
            FlightNumber = flightNumber?.ToUpperInvariant(),
            DepartureTime = departureTime,
            ArrivalTime = arrivalTime,
            Stops = stops,
            CollectedAt = collectedAt == default ? DateTime.UtcNow : collectedAt
        };
        
        return snapshot;
    }

    public TimeSpan? Duration => 
        DepartureTime.HasValue && ArrivalTime.HasValue 
            ? ArrivalTime.Value - DepartureTime.Value 
            : null;

    public bool IsDirect => Stops == 0;

    public override string ToString()
    {
        return $"{AirlineCode} {FlightNumber} {Cabin} {Price.Amount} {Price.Currency} ({Stops} stops)";
    }
}
