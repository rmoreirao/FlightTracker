using FlightTracker.Domain.Enums;
using FlightTracker.Domain.ValueObjects;

namespace FlightTracker.Domain.Entities;

/// <summary>
/// Itinerary leg referencing a concrete Flight entity (denormalized data for performance and immutability of snapshot).
/// </summary>
public class ItineraryLeg
{
    public Guid ItineraryId { get; internal set; }
    public int Sequence { get; private set; }
    public Guid FlightId { get; private set; }
    public string FlightNumber { get; private set; } = string.Empty;
    public string AirlineCode { get; private set; } = string.Empty;
    public string OriginCode { get; private set; } = string.Empty;
    public string DestinationCode { get; private set; } = string.Empty;
    public DateTime DepartureUtc { get; private set; }
    public DateTime ArrivalUtc { get; private set; }
    public Money PriceComponent { get; private set; } = null!;
    public CabinClass CabinClass { get; private set; }
    public LegDirection Direction { get; private set; }

    public TimeSpan Duration => ArrivalUtc - DepartureUtc;

    // Public constructor used when building a new aggregate (ItineraryId will be injected by the aggregate factory)
    public ItineraryLeg(int sequence, Guid flightId, string flightNumber, string airlineCode,
        string originCode, string destinationCode, DateTime departureUtc, DateTime arrivalUtc,
        Money priceComponent, CabinClass cabinClass, LegDirection direction)
    {
        if (sequence < 0) throw new ArgumentOutOfRangeException(nameof(sequence));
        if (flightId == Guid.Empty) throw new ArgumentException("FlightId required", nameof(flightId));
        if (string.IsNullOrWhiteSpace(flightNumber)) throw new ArgumentException("Flight number required", nameof(flightNumber));
        if (string.IsNullOrWhiteSpace(airlineCode)) throw new ArgumentException("Airline code required", nameof(airlineCode));
        if (string.IsNullOrWhiteSpace(originCode)) throw new ArgumentException("Origin required", nameof(originCode));
        if (string.IsNullOrWhiteSpace(destinationCode)) throw new ArgumentException("Destination required", nameof(destinationCode));
        if (arrivalUtc <= departureUtc) throw new ArgumentException("Arrival must be after departure");
        if (priceComponent is null) throw new ArgumentNullException(nameof(priceComponent));
        Sequence = sequence;
        FlightId = flightId;
        FlightNumber = flightNumber.ToUpperInvariant();
        AirlineCode = airlineCode.ToUpperInvariant();
        OriginCode = originCode.ToUpperInvariant();
        DestinationCode = destinationCode.ToUpperInvariant();
        DepartureUtc = EnsureUtc(departureUtc);
        ArrivalUtc = EnsureUtc(arrivalUtc);
        PriceComponent = priceComponent;
        CabinClass = cabinClass;
        Direction = direction;
    }

    private ItineraryLeg() {}

    private static DateTime EnsureUtc(DateTime dt)
    {
        return dt.Kind switch
        {
            DateTimeKind.Utc => dt,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dt, DateTimeKind.Utc),
            _ => dt.ToUniversalTime()
        };
    }
}
