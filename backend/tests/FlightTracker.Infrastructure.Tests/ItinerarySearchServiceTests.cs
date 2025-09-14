using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Enums;
using FlightTracker.Domain.Repositories;
using FlightTracker.Domain.Services;
using FlightTracker.Domain.ValueObjects;
using FlightTracker.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace FlightTracker.Infrastructure.Tests;

public class ItinerarySearchServiceTests
{
    private static Flight CreateFlight(string num, string org, string dest, DateTime dep, DateTime arr, decimal price, CabinClass cabin = CabinClass.Economy)
    {
        var origin = new Airport(org, org+" Airport", org+" City", "Country");
        var destA = new Airport(dest, dest+" Airport", dest+" City", "Country");
        return new Flight(num, "AA", "Airline", origin, destA, dep, arr, new Money(price, "USD"), cabin);
    }

    [Fact]
    public async Task RoundTrip_PairsOutboundAndReturn()
    {
        var flightRepo = new Mock<IFlightRepository>();
        var itinRepo = new Mock<IItineraryRepository>();
        var depDate = DateTime.UtcNow.AddDays(3).Date;
        var retDate = depDate.AddDays(5);
        var outbound = CreateFlight("AA100", "AAA", "BBB", depDate.AddHours(8), depDate.AddHours(10), 100);
        var inbound = CreateFlight("AA101", "BBB", "AAA", retDate.AddHours(9), retDate.AddHours(11), 120);
        flightRepo.Setup(r => r.SearchAsync("AAA","BBB", depDate, null, It.IsAny<FlightSearchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Flight>{ outbound });
        flightRepo.Setup(r => r.SearchAsync("BBB","AAA", retDate, null, It.IsAny<FlightSearchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Flight>{ inbound });

        IItinerarySearchService service = new ItinerarySearchService(flightRepo.Object, itinRepo.Object, NullLogger<ItinerarySearchService>.Instance);
        var itineraries = await service.SearchAsync("AAA","BBB", depDate, retDate, ItinerarySearchOptions.Default);
        Assert.Single(itineraries);
        Assert.True(itineraries.First().IsRoundTrip);
    }

    [Fact]
    public async Task OneWay_ReturnsEachFlightAsItinerary()
    {
        var flightRepo = new Mock<IFlightRepository>();
        var itinRepo = new Mock<IItineraryRepository>();
        var depDate = DateTime.UtcNow.AddDays(2).Date;
        var f1 = CreateFlight("AA200", "AAA", "BBB", depDate.AddHours(7), depDate.AddHours(9), 90);
        var f2 = CreateFlight("AA201", "AAA", "BBB", depDate.AddHours(12), depDate.AddHours(14), 110);
        flightRepo.Setup(r => r.SearchAsync("AAA","BBB", depDate, null, It.IsAny<FlightSearchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Flight>{ f1, f2 });

        IItinerarySearchService service = new ItinerarySearchService(flightRepo.Object, itinRepo.Object, NullLogger<ItinerarySearchService>.Instance);
        var itineraries = await service.SearchAsync("AAA","BBB", depDate, null, ItinerarySearchOptions.Default);
        Assert.Equal(2, itineraries.Count);
        Assert.All(itineraries, i => Assert.False(i.IsRoundTrip));
    }
}
