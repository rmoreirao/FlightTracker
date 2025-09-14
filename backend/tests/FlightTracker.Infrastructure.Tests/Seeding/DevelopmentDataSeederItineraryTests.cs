using System.Linq;using System.Threading.Tasks;using FlightTracker.Infrastructure.Configuration;using FlightTracker.Infrastructure.Services;using Microsoft.EntityFrameworkCore;using Microsoft.Extensions.Logging.Abstractions;using Microsoft.Extensions.Options;using Xunit;

namespace FlightTracker.Infrastructure.Tests.Seeding;

public class DevelopmentDataSeederItineraryTests
{
    private FlightDbContext CreateContext() => new(new DbContextOptionsBuilder<FlightDbContext>()
        .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
        .Options);

    [Fact]
    public async Task Seeder_Generates_OneWay_And_RoundTrip_Itineraries()
    {
        var ctx = CreateContext();
        var options = Options.Create(new DatabaseOptions
        {
            SeedTestDataOnStartup = true,
            ForceReseedData = true,
            GenerateItineraries = true,
            MaxOneWayPerRoutePerDay = 3,
            MaxRoundTripsPerRoutePerDay = 2,
            MinReturnTripDays = 2,
            MaxReturnTripDays = 5
        });
        var seeder = new DevelopmentDataSeeder(ctx, new NullLogger<DevelopmentDataSeeder>(), options);

        await seeder.SeedAirportsAsync();
        await seeder.SeedAirlinesAsync();
        await seeder.SeedFlightsAsync();
        await seeder.SeedItinerariesAsyncWrapperForTest();

        var itineraries = ctx.Itineraries.Include(i => i.Legs).ToList();

        Assert.NotEmpty(itineraries);
        Assert.Contains(itineraries, i => i.Legs.Count == 1);
        Assert.Contains(itineraries, i => i.Legs.Count == 2 && i.Legs.Any(l => l.Direction == Domain.Enums.LegDirection.Return));
    }
}
