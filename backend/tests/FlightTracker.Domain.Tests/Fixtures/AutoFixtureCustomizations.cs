using AutoFixture;
using AutoFixture.Dsl;
using FlightTracker.Domain.Entities;
using FlightTracker.Domain.ValueObjects;
using FlightTracker.Domain.Enums;

namespace FlightTracker.Domain.Tests.Fixtures;

/// <summary>
/// AutoFixture customizations for domain objects to ensure valid test data generation
/// </summary>
public static class AutoFixtureCustomizations
{
    public static IFixture CreateDomainFixture()
    {
        var fixture = new Fixture();
        
        // Configure Money value object
        fixture.Customize<Money>(composer => composer
            .FromFactory((decimal amount, string currency) => 
                new Money(Math.Abs(amount) % 10000m, ValidCurrencies.GetRandom()))
            .OmitAutoProperties());

        // Configure DateRange value object with valid dates
        fixture.Customize<DateRange>(composer => composer
            .FromFactory((DateTime start) =>
            {
                var startDate = start.Date;
                var endDate = startDate.AddDays(fixture.Create<int>() % 30 + 1);
                return new DateRange(startDate, endDate);
            })
            .OmitAutoProperties());

        // Configure RouteKey with valid airport codes
        fixture.Customize<RouteKey>(composer => composer
            .FromFactory(() =>
            {
                var airports = ValidAirportCodes.GetRandom(2);
                return new RouteKey(airports[0], airports[1]);
            })
            .OmitAutoProperties());

        // Configure Airport with valid data
        fixture.Customize<Airport>(composer => composer
            .FromFactory((string name, string city, string country) =>
            {
                var code = ValidAirportCodes.GetRandom();
                return new Airport(code, name ?? $"{code} Airport", city ?? $"{code} City", country ?? "TestCountry");
            })
            .OmitAutoProperties());

        // Configure Airline with valid data
        fixture.Customize<Airline>(composer => composer
            .FromFactory((string name) =>
            {
                var code = ValidAirlineCodes.GetRandom();
                return new Airline(code, name ?? $"{code} Airlines");
            })
            .OmitAutoProperties());

        // Configure Flight with valid relationships
        fixture.Customize<Flight>(composer => composer
            .Do(flight =>
            {
                // Ensure flight has at least one segment
                if (!flight.Segments.Any())
                {
                    var segment = fixture.Create<FlightSegment>();
                    flight.AddSegment(segment);
                }
            }));

        // Configure FlightSegment with valid data
        fixture.Customize<FlightSegment>(composer => composer
            .FromFactory((string flightNumber, DateTime departureTime, TimeSpan duration) =>
            {
                var airline = fixture.Create<Airline>();
                var origin = fixture.Create<Airport>();
                var destination = fixture.Create<Airport>();
                var validDeparture = departureTime.Date.AddHours(fixture.Create<int>() % 24);
                var validDuration = TimeSpan.FromMinutes(Math.Abs(duration.TotalMinutes) % 600 + 60); // 1-10 hours
                
                return new FlightSegment(
                    flightNumber ?? $"{airline.Code}{fixture.Create<int>() % 9999:D4}",
                    airline,
                    origin,
                    destination,
                    validDeparture,
                    validDeparture.Add(validDuration));
            })
            .OmitAutoProperties());

        // Configure FlightQuery with valid search parameters
        fixture.Customize<FlightQuery>(composer => composer
            .FromFactory(() =>
            {
                var origin = ValidAirportCodes.GetRandom();
                var destination = ValidAirportCodes.GetRandom();
                while (destination == origin)
                {
                    destination = ValidAirportCodes.GetRandom();
                }
                
                var departureDate = DateTime.Today.AddDays(fixture.Create<int>() % 365 + 1);
                var returnDate = fixture.Create<bool>() ? departureDate.AddDays(fixture.Create<int>() % 30 + 1) : (DateTime?)null;
                
                return new FlightQuery(
                    origin,
                    destination,
                    departureDate,
                    returnDate,
                    fixture.Create<int>() % 9 + 1, // 1-9 passengers
                    fixture.Create<CabinClass>());
            })
            .OmitAutoProperties());

        // Configure PriceSnapshot with valid data
        fixture.Customize<PriceSnapshot>(composer => composer
            .FromFactory(() =>
            {
                var flight = fixture.Create<Flight>();
                var price = fixture.Create<Money>();
                var cabinClass = fixture.Create<CabinClass>();
                
                return new PriceSnapshot(flight.Id, price, cabinClass, DateTime.UtcNow);
            })
            .OmitAutoProperties());

        return fixture;
    }
}

/// <summary>
/// Helper class for valid airport codes used in testing
/// </summary>
public static class ValidAirportCodes
{
    private static readonly string[] Codes = 
    {
        "JFK", "LAX", "ORD", "DFW", "DEN", "LAS", "PHX", "SEA", "MIA", "CLT",
        "MCO", "EWR", "SFO", "DTW", "MSP", "BOS", "PHL", "LGA", "BWI", "IAD",
        "FLL", "DCA", "MDW", "TPA", "SAN", "HNL", "PDX", "STL", "IAH", "ATL"
    };

    public static string GetRandom() => Codes[Random.Shared.Next(Codes.Length)];
    
    public static string[] GetRandom(int count) => 
        Codes.OrderBy(_ => Random.Shared.Next()).Take(count).ToArray();
}

/// <summary>
/// Helper class for valid airline codes used in testing
/// </summary>
public static class ValidAirlineCodes
{
    private static readonly string[] Codes = 
    {
        "AA", "DL", "UA", "WN", "AS", "B6", "NK", "F9", "G4", "SY"
    };

    public static string GetRandom() => Codes[Random.Shared.Next(Codes.Length)];
}

/// <summary>
/// Helper class for valid currency codes used in testing
/// </summary>
public static class ValidCurrencies
{
    private static readonly string[] Codes = 
    {
        "USD", "EUR", "GBP", "CAD", "AUD", "JPY"
    };

    public static string GetRandom() => Codes[Random.Shared.Next(Codes.Length)];
}
