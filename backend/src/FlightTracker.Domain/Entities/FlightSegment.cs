using FlightTracker.Domain.Enums;

namespace FlightTracker.Domain.Entities;

/// <summary>
/// Flight segment representing individual legs of a journey
/// </summary>
public class FlightSegment
{
    public int Id { get; private set; }
    public string FlightNumber { get; private set; }
    public string AirlineCode { get; private set; }
    public Airport Origin { get; private set; }
    public Airport Destination { get; private set; }
    public DateTime DepartureTime { get; private set; }
    public DateTime ArrivalTime { get; private set; }
    public TimeSpan Duration => ArrivalTime - DepartureTime;
    public string? AircraftType { get; private set; }
    public int SegmentOrder { get; private set; }
    public FlightStatus Status { get; private set; }

    public FlightSegment(
        string flightNumber,
        string airlineCode,
        Airport origin,
        Airport destination,
        DateTime departureTime,
        DateTime arrivalTime,
        int segmentOrder,
        string? aircraftType = null,
        FlightStatus status = FlightStatus.Scheduled)
    {
        if (string.IsNullOrWhiteSpace(flightNumber))
            throw new ArgumentException("Flight number is required", nameof(flightNumber));
        
        if (string.IsNullOrWhiteSpace(airlineCode))
            throw new ArgumentException("Airline code is required", nameof(airlineCode));
        
        if (origin == null)
            throw new ArgumentNullException(nameof(origin));
        
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));
        
        if (origin.Code == destination.Code)
            throw new ArgumentException("Origin and destination cannot be the same");
        
        if (arrivalTime <= departureTime)
            throw new ArgumentException("Arrival time must be after departure time");
        
        if (segmentOrder < 1)
            throw new ArgumentException("Segment order must be positive", nameof(segmentOrder));

        FlightNumber = flightNumber.ToUpperInvariant();
        AirlineCode = airlineCode.ToUpperInvariant();
        Origin = origin;
        Destination = destination;
        DepartureTime = departureTime;
        ArrivalTime = arrivalTime;
        SegmentOrder = segmentOrder;
        AircraftType = aircraftType;
        Status = status;
    }

    // For EF Core
    private FlightSegment() { }

    public void UpdateStatus(FlightStatus newStatus)
    {
        Status = newStatus;
    }

    public void UpdateAircraftType(string aircraftType)
    {
        AircraftType = aircraftType;
    }

    public override string ToString()
    {
        return $"{FlightNumber} {Origin.Code}-{Destination.Code} {DepartureTime:HH:mm}-{ArrivalTime:HH:mm}";
    }
}
