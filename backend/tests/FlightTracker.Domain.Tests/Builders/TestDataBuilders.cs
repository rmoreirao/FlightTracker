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
        // Use the first segment for primary flight info, or create default
        var primarySegment = _segments.FirstOrDefault() ?? 
            FlightSegmentBuilder.Create().Build();

        var flight = new Flight(
            primarySegment.FlightNumber,
            primarySegment.AirlineCode,
            "Test Airline", // Default airline name
            primarySegment.Origin ?? new Airport("LAX", "Los Angeles International", "Los Angeles", "USA", 34.0522m, -118.2437m),
            primarySegment.Destination ?? new Airport("JFK", "John F. Kennedy International", "New York", "USA", 40.6413m, -73.7781m),
            primarySegment.DepartureTime,
            primarySegment.ArrivalTime,
            new Money(299.99m, "USD"), // Default price
            CabinClass.Economy, // Default cabin class
            null, // No deep link
            _status);

        // Add additional segments if any
        foreach (var segment in _segments.Skip(1))
        {
            flight.AddSegment(segment);
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
    private Airport _origin = new("JFK", "John F. Kennedy International", "New York", "USA", 40.6413m, -73.7781m);
    private Airport _destination = new("LAX", "Los Angeles International", "Los Angeles", "USA", 34.0522m, -118.2437m);
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
        _origin = new Airport(originCode, $"{originCode} Airport", $"{originCode} City", "USA", 0m, 0m);
        _destination = new Airport(destinationCode, $"{destinationCode} Airport", $"{destinationCode} City", "USA", 0m, 0m);
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
        return new FlightSegment(
            _flightNumber, 
            _airline.Code, 
            _origin, 
            _destination, 
            _departureTime, 
            _arrivalTime,
            1); // Default segment order
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
        return new FlightQuery(_origin, _destination, _departureDate, _returnDate);
    }
}

/// <summary>
/// Test data builder for PriceSnapshot entities
/// </summary>
public class PriceSnapshotBuilder
{
    private Guid _queryId = Guid.NewGuid();
    private string _airlineCode = "AA";
    private Money _price = new(500m, "USD");
    private CabinClass _cabinClass = CabinClass.Economy;
    private string? _deepLink;
    private string? _flightNumber = "AA1234";
    private DateTime? _departureTime;
    private DateTime? _arrivalTime;
    private int _stops = 0;

    public static PriceSnapshotBuilder Create() => new();

    public PriceSnapshotBuilder ForQuery(Guid queryId)
    {
        _queryId = queryId;
        return this;
    }

    public PriceSnapshotBuilder ForQuery(FlightQuery query)
    {
        _queryId = query.Id;
        return this;
    }

    public PriceSnapshotBuilder WithAirline(string airlineCode)
    {
        _airlineCode = airlineCode;
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

    public PriceSnapshotBuilder WithFlightDetails(string? flightNumber, DateTime? departureTime = null, DateTime? arrivalTime = null, int stops = 0)
    {
        _flightNumber = flightNumber;
        _departureTime = departureTime;
        _arrivalTime = arrivalTime;
        _stops = stops;
        return this;
    }

    public PriceSnapshotBuilder WithDeepLink(string? deepLink)
    {
        _deepLink = deepLink;
        return this;
    }

    public PriceSnapshot Build()
    {
        return new PriceSnapshot(
            _queryId,
            _airlineCode,
            _cabinClass,
            _price,
            _deepLink,
            _flightNumber,
            _departureTime,
            _arrivalTime,
            _stops);
    }
}
