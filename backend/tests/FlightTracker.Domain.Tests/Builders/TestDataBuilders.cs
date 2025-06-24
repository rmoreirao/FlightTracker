using FlightTracker.Domain.Entities;
using FlightTracker.Domain.ValueObjects;
using FlightTracker.Domain.Enums;

namespace FlightTracker.Domain.Tests.Builders;

/// <summary>
/// Test data builder for Flight entities
/// </summary>
public class FlightBuilder
{
    private readonly List<FlightSegment> _segments = new();
    private FlightStatus _status = FlightStatus.Scheduled;
    private string? _confirmationCode;

    public static FlightBuilder Create() => new();

    public FlightBuilder WithSegment(FlightSegment segment)
    {
        _segments.Add(segment);
        return this;
    }

    public FlightBuilder WithSegment(Action<FlightSegmentBuilder> configure)
    {
        var builder = FlightSegmentBuilder.Create();
        configure(builder);
        return WithSegment(builder.Build());
    }

    public FlightBuilder WithStatus(FlightStatus status)
    {
        _status = status;
        return this;
    }

    public FlightBuilder WithConfirmationCode(string confirmationCode)
    {
        _confirmationCode = confirmationCode;
        return this;
    }

    public FlightBuilder AsRoundTrip(string originCode, string destinationCode, DateTime departureDate, DateTime returnDate)
    {
        var outbound = FlightSegmentBuilder.Create()
            .WithRoute(originCode, destinationCode)
            .WithDepartureTime(departureDate)
            .Build();

        var returning = FlightSegmentBuilder.Create()
            .WithRoute(destinationCode, originCode)
            .WithDepartureTime(returnDate)
            .Build();

        return WithSegment(outbound).WithSegment(returning);
    }

    public FlightBuilder AsOneWay(string originCode, string destinationCode, DateTime departureDate)
    {
        var segment = FlightSegmentBuilder.Create()
            .WithRoute(originCode, destinationCode)
            .WithDepartureTime(departureDate)
            .Build();

        return WithSegment(segment);
    }

    public Flight Build()
    {
        var flight = new Flight();
        
        foreach (var segment in _segments)
        {
            flight.AddSegment(segment);
        }

        if (_confirmationCode != null)
        {
            flight.UpdateStatus(_status, _confirmationCode);
        }
        else
        {
            flight.UpdateStatus(_status);
        }

        return flight;
    }
}

/// <summary>
/// Test data builder for FlightSegment entities
/// </summary>
public class FlightSegmentBuilder
{
    private string _flightNumber = "AA1234";
    private Airline _airline = new("AA", "American Airlines");
    private Airport _origin = new("JFK", "John F. Kennedy International", "New York", "USA");
    private Airport _destination = new("LAX", "Los Angeles International", "Los Angeles", "USA");
    private DateTime _departureTime = DateTime.Today.AddDays(1).AddHours(10);
    private DateTime _arrivalTime = DateTime.Today.AddDays(1).AddHours(16);

    public static FlightSegmentBuilder Create() => new();

    public FlightSegmentBuilder WithFlightNumber(string flightNumber)
    {
        _flightNumber = flightNumber;
        return this;
    }

    public FlightSegmentBuilder WithAirline(string code, string name)
    {
        _airline = new Airline(code, name);
        return this;
    }

    public FlightSegmentBuilder WithAirline(Airline airline)
    {
        _airline = airline;
        return this;
    }

    public FlightSegmentBuilder WithRoute(string originCode, string destinationCode)
    {
        _origin = new Airport(originCode, $"{originCode} Airport", $"{originCode} City", "USA");
        _destination = new Airport(destinationCode, $"{destinationCode} Airport", $"{destinationCode} City", "USA");
        return this;
    }

    public FlightSegmentBuilder WithOrigin(Airport origin)
    {
        _origin = origin;
        return this;
    }

    public FlightSegmentBuilder WithDestination(Airport destination)
    {
        _destination = destination;
        return this;
    }

    public FlightSegmentBuilder WithDepartureTime(DateTime departureTime)
    {
        _departureTime = departureTime;
        return this;
    }

    public FlightSegmentBuilder WithArrivalTime(DateTime arrivalTime)
    {
        _arrivalTime = arrivalTime;
        return this;
    }

    public FlightSegmentBuilder WithDuration(TimeSpan duration)
    {
        _arrivalTime = _departureTime.Add(duration);
        return this;
    }

    public FlightSegment Build()
    {
        return new FlightSegment(_flightNumber, _airline, _origin, _destination, _departureTime, _arrivalTime);
    }
}

/// <summary>
/// Test data builder for FlightQuery entities
/// </summary>
public class FlightQueryBuilder
{
    private string _origin = "JFK";
    private string _destination = "LAX";
    private DateTime _departureDate = DateTime.Today.AddDays(7);
    private DateTime? _returnDate;
    private int _passengers = 1;
    private CabinClass _cabinClass = CabinClass.Economy;

    public static FlightQueryBuilder Create() => new();

    public FlightQueryBuilder WithRoute(string origin, string destination)
    {
        _origin = origin;
        _destination = destination;
        return this;
    }

    public FlightQueryBuilder WithDepartureDate(DateTime departureDate)
    {
        _departureDate = departureDate;
        return this;
    }

    public FlightQueryBuilder WithReturnDate(DateTime? returnDate)
    {
        _returnDate = returnDate;
        return this;
    }

    public FlightQueryBuilder AsRoundTrip(DateTime returnDate)
    {
        _returnDate = returnDate;
        return this;
    }

    public FlightQueryBuilder AsOneWay()
    {
        _returnDate = null;
        return this;
    }

    public FlightQueryBuilder WithPassengers(int passengers)
    {
        _passengers = passengers;
        return this;
    }

    public FlightQueryBuilder WithCabinClass(CabinClass cabinClass)
    {
        _cabinClass = cabinClass;
        return this;
    }

    public FlightQuery Build()
    {
        return new FlightQuery(_origin, _destination, _departureDate, _returnDate, _passengers, _cabinClass);
    }
}

/// <summary>
/// Test data builder for PriceSnapshot entities
/// </summary>
public class PriceSnapshotBuilder
{
    private Guid _flightId = Guid.NewGuid();
    private Money _price = new(500m, "USD");
    private CabinClass _cabinClass = CabinClass.Economy;
    private DateTime _timestamp = DateTime.UtcNow;

    public static PriceSnapshotBuilder Create() => new();

    public PriceSnapshotBuilder ForFlight(Guid flightId)
    {
        _flightId = flightId;
        return this;
    }

    public PriceSnapshotBuilder ForFlight(Flight flight)
    {
        _flightId = flight.Id;
        return this;
    }

    public PriceSnapshotBuilder WithPrice(decimal amount, string currency = "USD")
    {
        _price = new Money(amount, currency);
        return this;
    }

    public PriceSnapshotBuilder WithPrice(Money price)
    {
        _price = price;
        return this;
    }

    public PriceSnapshotBuilder WithCabinClass(CabinClass cabinClass)
    {
        _cabinClass = cabinClass;
        return this;
    }

    public PriceSnapshotBuilder WithTimestamp(DateTime timestamp)
    {
        _timestamp = timestamp;
        return this;
    }

    public PriceSnapshot Build()
    {
        return new PriceSnapshot(_flightId, _price, _cabinClass, _timestamp);
    }
}
