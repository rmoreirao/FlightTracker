using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Enums;
using FlightTracker.Domain.ValueObjects;
using Xunit;

namespace FlightTracker.Domain.Tests;

public class ItineraryTests
{
    private static Flight CreateFlight(string num, string org, string dest, DateTime dep, DateTime arr, decimal price)
    {
        var origin = new Airport(org, org+" Airport", org+" City", "Country");
        var destA = new Airport(dest, dest+" Airport", dest+" City", "Country");
        return new Flight(num, "AA", "Airline", origin, destA, dep, arr, new Money(price, "USD"), CabinClass.Economy);
    }

    [Fact]
    public void Create_OneWay_Itinerary_Succeeds()
    {
        var f = CreateFlight("AA100", "AAA", "BBB", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(2), 100);
        var leg = new ItineraryLeg(0, f.Id, f.FlightNumber, f.AirlineCode, f.Origin!.Code, f.Destination!.Code, f.DepartureTime, f.ArrivalTime, f.Price, f.CabinClass, LegDirection.Outbound);
        var itin = Itinerary.Create(new[]{leg});
        Assert.False(itin.IsRoundTrip);
        Assert.Equal(100, itin.TotalPrice.Amount);
    }

    [Fact]
    public void Create_RoundTrip_EndsWhereStarted()
    {
        var outbound = CreateFlight("AA101", "AAA", "BBB", DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(2).AddHours(2), 120);
        var inbound = CreateFlight("AA102", "BBB", "AAA", DateTime.UtcNow.AddDays(5), DateTime.UtcNow.AddDays(5).AddHours(2), 130);
        var legs = new []
        {
            new ItineraryLeg(0, outbound.Id, outbound.FlightNumber, outbound.AirlineCode, outbound.Origin!.Code, outbound.Destination!.Code, outbound.DepartureTime, outbound.ArrivalTime, outbound.Price, outbound.CabinClass, LegDirection.Outbound),
            new ItineraryLeg(1, inbound.Id, inbound.FlightNumber, inbound.AirlineCode, inbound.Origin!.Code, inbound.Destination!.Code, inbound.DepartureTime, inbound.ArrivalTime, inbound.Price, inbound.CabinClass, LegDirection.Return)
        };
        var itin = Itinerary.Create(legs);
        Assert.True(itin.IsRoundTrip);
        Assert.Equal("AAA", itin.Origin);
        Assert.Equal("AAA", itin.FinalDestination);
        Assert.Equal(250, itin.TotalPrice.Amount);
    }

    [Fact]
    public void Create_Fails_When_Leg_Overlap()
    {
        var f1 = CreateFlight("AA200", "AAA", "BBB", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(2), 100);
        // Overlapping second leg departs before first arrives
        var f2 = CreateFlight("AA201", "BBB", "CCC", f1.ArrivalTime.AddMinutes(-30), f1.ArrivalTime.AddHours(1), 150);
        var legs = new[]
        {
            new ItineraryLeg(0, f1.Id, f1.FlightNumber, f1.AirlineCode, f1.Origin!.Code, f1.Destination!.Code, f1.DepartureTime, f1.ArrivalTime, f1.Price, f1.CabinClass, LegDirection.Outbound),
            new ItineraryLeg(1, f2.Id, f2.FlightNumber, f2.AirlineCode, f2.Origin!.Code, f2.Destination!.Code, f2.DepartureTime, f2.ArrivalTime, f2.Price, f2.CabinClass, LegDirection.Outbound)
        };
        Assert.Throws<InvalidOperationException>(() => Itinerary.Create(legs));
    }

    [Fact]
    public void Create_Fails_When_Currency_Mismatch()
    {
        var f1 = CreateFlight("AA300", "AAA", "BBB", DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(3).AddHours(2), 100);
        var f2 = CreateFlight("AA301", "BBB", "CCC", DateTime.UtcNow.AddDays(4), DateTime.UtcNow.AddDays(4).AddHours(2), 200);
        // Alter second leg price currency
        var priceDifferent = new Money(200, "EUR");
        var legs = new[]
        {
            new ItineraryLeg(0, f1.Id, f1.FlightNumber, f1.AirlineCode, f1.Origin!.Code, f1.Destination!.Code, f1.DepartureTime, f1.ArrivalTime, f1.Price, f1.CabinClass, LegDirection.Outbound),
            new ItineraryLeg(1, f2.Id, f2.FlightNumber, f2.AirlineCode, f2.Origin!.Code, f2.Destination!.Code, f2.DepartureTime, f2.ArrivalTime, priceDifferent, f2.CabinClass, LegDirection.Outbound)
        };
        Assert.Throws<InvalidOperationException>(() => Itinerary.Create(legs));
    }
}
