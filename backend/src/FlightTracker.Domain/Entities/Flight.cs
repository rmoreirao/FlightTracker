using FlightTracker.Domain.Enums;
using FlightTracker.Domain.ValueObjects;

namespace FlightTracker.Domain.Entities;

/// <summary>
/// Flight entity representing a flight offering with pricing and routing information
/// </summary>
public class Flight
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FlightNumber { get; private set; } = string.Empty;
    public string AirlineCode { get; private set; } = string.Empty;
    public string AirlineName { get; private set; } = string.Empty;
    public Airport? Origin { get; private set; }
    public Airport? Destination { get; private set; }
    public DateTime DepartureTime { get; private set; }
    public DateTime ArrivalTime { get; private set; }
    public TimeSpan Duration => ArrivalTime - DepartureTime;
    public List<FlightSegment> Segments { get; private set; } = new();
    public Money Price { get; private set; } = null!;
    public CabinClass CabinClass { get; private set; }
    public string? DeepLink { get; private set; }
    public int Stops => Math.Max(0, Segments.Count - 1);
    public FlightStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public Flight(
        string flightNumber,
        string airlineCode,
        string airlineName,
        Airport origin,
        Airport destination,
        DateTime departureTime,
        DateTime arrivalTime,
        Money price,
        CabinClass cabinClass,
        string? deepLink = null,
        FlightStatus status = FlightStatus.Scheduled)
    {
        if (string.IsNullOrWhiteSpace(flightNumber))
            throw new ArgumentException("Flight number is required", nameof(flightNumber));
        
        if (string.IsNullOrWhiteSpace(airlineCode))
            throw new ArgumentException("Airline code is required", nameof(airlineCode));
        
        if (string.IsNullOrWhiteSpace(airlineName))
            throw new ArgumentException("Airline name is required", nameof(airlineName));
        
        if (origin == null)
            throw new ArgumentNullException(nameof(origin));
        
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));
        
        if (origin.Code == destination.Code)
            throw new ArgumentException("Origin and destination cannot be the same");
        
        if (arrivalTime <= departureTime)
            throw new ArgumentException("Arrival time must be after departure time");
        
        if (price == null)
            throw new ArgumentNullException(nameof(price));

        FlightNumber = flightNumber.ToUpperInvariant();
        AirlineCode = airlineCode.ToUpperInvariant();
        AirlineName = airlineName;
        Origin = origin;
        Destination = destination;
        DepartureTime = departureTime;
        ArrivalTime = arrivalTime;
        Price = price;
        CabinClass = cabinClass;
        DeepLink = deepLink;
        Status = status;
    }

    // For EF Core
    private Flight() { }

    public void AddSegment(FlightSegment segment)
    {
        if (segment == null)
            throw new ArgumentNullException(nameof(segment));
        
        Segments.Add(segment);
    }

    public void UpdatePrice(Money newPrice)
    {
        Price = newPrice ?? throw new ArgumentNullException(nameof(newPrice));
    }

    public void UpdateStatus(FlightStatus newStatus)
    {
        Status = newStatus;
    }

    public void UpdateDeepLink(string? deepLink)
    {
        DeepLink = deepLink;
    }

    public bool IsDirect => Segments.Count <= 1;

    public bool IsInternational => Origin?.Country != Destination?.Country;

    public override string ToString()
    {
        return $"{FlightNumber} {Origin?.Code}-{Destination?.Code} {DepartureTime:HH:mm}-{ArrivalTime:HH:mm} {Price.Amount} {Price.Currency}";
    }
}
