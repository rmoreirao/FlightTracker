using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Enums;
using FlightTracker.Domain.Repositories;
using FlightTracker.Domain.ValueObjects;
using FlightTracker.Infrastructure;
using FlightTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FlightTracker.Infrastructure.Tests.Repositories;

public class ItineraryRepositoryTests
{
    private static Flight CreateFlight(string num, string org, string dest, DateTime dep, DateTime arr, decimal price, CabinClass cabin = CabinClass.Economy)
    {
        var origin = new Airport(org, org+" Airport", org+" City", "Country");
        var destA = new Airport(dest, dest+" Airport", dest+" City", "Country");
        return new Flight(num, "AA", "Airline", origin, destA, dep, arr, new Money(price, "USD"), cabin);
    }

    private FlightDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<FlightDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new FlightDbContext(options);
    }

    [Fact]
    public async Task Save_And_Search_OneWay_Itinerary()
    {
        using var ctx = CreateContext();
    var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<EfItineraryRepository>();
    IItineraryRepository repo = new EfItineraryRepository(ctx, logger);

        var f = CreateFlight("AA300", "AAA", "BBB", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(2), 150);
    var leg = new ItineraryLeg(0, f.Id, f.FlightNumber, f.AirlineCode, f.Origin!.Code, f.Destination!.Code, f.DepartureTime, f.ArrivalTime, f.Price, f.CabinClass, LegDirection.Outbound);
        // Itinerary.Create assigns itineraryId to legs
        var itin = Itinerary.Create(new[]{leg});
    await repo.AddAsync(itin, CancellationToken.None);

        var results = await repo.SearchAsync("AAA", "BBB", f.DepartureTime.Date, null, ItinerarySearchOptions.Default, CancellationToken.None);
        Assert.Single(results);
        Assert.False(results[0].IsRoundTrip);
    }

    [Fact]
    public async Task Save_And_Search_RoundTrip_Itinerary()
    {
        using var ctx = CreateContext();
    var logger = new Microsoft.Extensions.Logging.Abstractions.NullLogger<EfItineraryRepository>();
    IItineraryRepository repo = new EfItineraryRepository(ctx, logger);

        var depDate = DateTime.UtcNow.AddDays(3).Date;
        var retDate = depDate.AddDays(5).Date;
        var outbound = CreateFlight("AA400", "AAA", "BBB", depDate.AddHours(9), depDate.AddHours(11), 120);
        var inbound = CreateFlight("AA401", "BBB", "AAA", retDate.AddHours(10), retDate.AddHours(12), 130);

        var legs = new []
        {
            new ItineraryLeg(0, outbound.Id, outbound.FlightNumber, outbound.AirlineCode, outbound.Origin!.Code, outbound.Destination!.Code, outbound.DepartureTime, outbound.ArrivalTime, outbound.Price, outbound.CabinClass, LegDirection.Outbound),
            new ItineraryLeg(1, inbound.Id, inbound.FlightNumber, inbound.AirlineCode, inbound.Origin!.Code, inbound.Destination!.Code, inbound.DepartureTime, inbound.ArrivalTime, inbound.Price, inbound.CabinClass, LegDirection.Return)
        };
        var itin = Itinerary.Create(legs);
    await repo.AddAsync(itin, CancellationToken.None);

        var options = ItinerarySearchOptions.Default with { }; // default filters
    // Search should specify outbound origin/destination (AAA->BBB) with return date
    var results = await repo.SearchAsync("AAA", "BBB", depDate, retDate, options, CancellationToken.None);
        Assert.Single(results);
        Assert.True(results[0].IsRoundTrip);
    }
}
