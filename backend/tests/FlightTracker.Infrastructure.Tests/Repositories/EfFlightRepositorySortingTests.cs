using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Enums;
using FlightTracker.Domain.ValueObjects;
using FlightTracker.Infrastructure.Repositories;
using FlightTracker.Infrastructure.Tests.Base;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FlightTracker.Infrastructure.Tests.Repositories;

public class EfFlightRepositorySortingTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;
    private readonly Mock<ILogger<EfFlightRepository>> _loggerMock;

    public EfFlightRepositorySortingTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
        _loggerMock = new Mock<ILogger<EfFlightRepository>>();
    }

    [Fact]
    public async Task SearchAsync_SortByPrice_ShouldReturnFlightsSortedByPrice()
    {
        // Arrange
        using var context = _databaseFixture.CreateContext();
        var repository = new EfFlightRepository(context, _loggerMock.Object);

        // Create test flights with different prices
        var flights = await SeedTestFlights(context);
        
        var searchOptions = FlightSearchOptions.Create(
            FlightSortBy.Price,
            SortOrder.Ascending);

        // Act
        var result = await repository.SearchAsync("LAX", "JFK", DateTime.UtcNow.Date.AddDays(1), searchOptions);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().BeInAscendingOrder(f => f.Price.Amount);
    }

    [Fact]
    public async Task SearchAsync_SortByPriceDescending_ShouldReturnFlightsSortedByPriceDesc()
    {
        // Arrange
        using var context = _databaseFixture.CreateContext();
        var repository = new EfFlightRepository(context, _loggerMock.Object);

        await SeedTestFlights(context);
        
        var searchOptions = FlightSearchOptions.Create(
            FlightSortBy.Price,
            SortOrder.Descending);

        // Act
        var result = await repository.SearchAsync("LAX", "JFK", DateTime.UtcNow.Date.AddDays(1), searchOptions);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().BeInDescendingOrder(f => f.Price.Amount);
    }

    [Fact]
    public async Task SearchAsync_SortByDuration_ShouldReturnFlightsSortedByDuration()
    {
        // Arrange
        using var context = _databaseFixture.CreateContext();
        var repository = new EfFlightRepository(context, _loggerMock.Object);

        await SeedTestFlights(context);
        
        var searchOptions = FlightSearchOptions.Create(
            FlightSortBy.Duration,
            SortOrder.Ascending);

        // Act
        var result = await repository.SearchAsync("LAX", "JFK", DateTime.UtcNow.Date.AddDays(1), searchOptions);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().BeInAscendingOrder(f => f.Duration);
    }

    [Fact]
    public async Task SearchAsync_SortByDepartureTime_ShouldReturnFlightsSortedByDepartureTime()
    {
        // Arrange
        using var context = _databaseFixture.CreateContext();
        var repository = new EfFlightRepository(context, _loggerMock.Object);

        await SeedTestFlights(context);
        
        var searchOptions = FlightSearchOptions.Create(
            FlightSortBy.DepartureTime,
            SortOrder.Ascending);

        // Act
        var result = await repository.SearchAsync("LAX", "JFK", DateTime.UtcNow.Date.AddDays(1), searchOptions);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().BeInAscendingOrder(f => f.DepartureTime);
    }

    [Fact]
    public async Task SearchAsync_SortByAirline_ShouldReturnFlightsSortedByAirlineCode()
    {
        // Arrange
        using var context = _databaseFixture.CreateContext();
        var repository = new EfFlightRepository(context, _loggerMock.Object);

        await SeedTestFlights(context);
        
        var searchOptions = FlightSearchOptions.Create(
            FlightSortBy.Airline,
            SortOrder.Ascending);

        // Act
        var result = await repository.SearchAsync("LAX", "JFK", DateTime.UtcNow.Date.AddDays(1), searchOptions);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().BeInAscendingOrder(f => f.AirlineCode);
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        using var context = _databaseFixture.CreateContext();
        var repository = new EfFlightRepository(context, _loggerMock.Object);

        await SeedTestFlights(context);
        
        var searchOptionsPage1 = FlightSearchOptions.Create(
            FlightSortBy.Price,
            SortOrder.Ascending,
            page: 1,
            pageSize: 2);

        var searchOptionsPage2 = FlightSearchOptions.Create(
            FlightSortBy.Price,
            SortOrder.Ascending,
            page: 2,
            pageSize: 2);

        // Act
        var page1 = await repository.SearchAsync("LAX", "JFK", DateTime.UtcNow.Date.AddDays(1), searchOptionsPage1);
        var page2 = await repository.SearchAsync("LAX", "JFK", DateTime.UtcNow.Date.AddDays(1), searchOptionsPage2);

        // Assert
        page1.Should().HaveCount(2);
        page2.Should().NotBeEmpty();
        
        // Ensure pages don't overlap
        var page1Ids = page1.Select(f => f.Id).ToList();
        var page2Ids = page2.Select(f => f.Id).ToList();
        page1Ids.Should().NotIntersectWith(page2Ids);
        
        // Ensure price ordering across pages
        if (page2.Any())
        {
            page1.Max(f => f.Price.Amount).Should().BeLessOrEqualTo(page2.Min(f => f.Price.Amount));
        }
    }

    [Fact]
    public async Task SearchAsync_WithoutSortingOptions_ShouldUseDefaultSorting()
    {
        // Arrange
        using var context = _databaseFixture.CreateContext();
        var repository = new EfFlightRepository(context, _loggerMock.Object);

        await SeedTestFlights(context);

        // Act
        var result = await repository.SearchAsync("LAX", "JFK", DateTime.UtcNow.Date.AddDays(1));

        // Assert
        result.Should().NotBeEmpty();
        result.Should().BeInAscendingOrder(f => f.DepartureTime);
    }

    private async Task<List<Flight>> SeedTestFlights(FlightDbContext context)
    {
        // Ensure we have clean test data
        var existingFlights = await context.Flights
            .Where(f => f.Origin!.Code == "LAX" && f.Destination!.Code == "JFK")
            .ToListAsync();
        
        if (existingFlights.Any())
        {
            return existingFlights;
        }

        // Create airports if they don't exist
        var lax = await context.Airports.FirstOrDefaultAsync(a => a.Code == "LAX");
        var jfk = await context.Airports.FirstOrDefaultAsync(a => a.Code == "JFK");

        if (lax == null)
        {
            lax = new Airport("LAX", "Los Angeles International Airport", "Los Angeles", "USA", 34.0522m, -118.2437m);
            context.Airports.Add(lax);
        }

        if (jfk == null)
        {
            jfk = new Airport("JFK", "John F. Kennedy International Airport", "New York", "USA", 40.6892m, -73.7781m);
            context.Airports.Add(jfk);
        }

        await context.SaveChangesAsync();

        // Create airlines if they don't exist
        var airlines = new[]
        {
            new Airline("AA", "American Airlines"),
            new Airline("DL", "Delta Air Lines"),
            new Airline("UA", "United Airlines")
        };

        foreach (var airline in airlines)
        {
            var existing = await context.Airlines.FirstOrDefaultAsync(a => a.Code == airline.Code);
            if (existing == null)
            {
                context.Airlines.Add(airline);
            }
        }

        await context.SaveChangesAsync();

        // Create test flights with varying prices, durations, and departure times
        var baseDate = DateTime.UtcNow.Date.AddDays(1);
        var flights = new List<Flight>
        {
            new Flight("AA100", "AA", "American Airlines", lax, jfk, 
                baseDate.AddHours(8), baseDate.AddHours(14), 
                new Money(299.99m, "USD"), CabinClass.Economy),
            
            new Flight("DL200", "DL", "Delta Air Lines", lax, jfk, 
                baseDate.AddHours(10), baseDate.AddHours(15), 
                new Money(399.99m, "USD"), CabinClass.Economy),
            
            new Flight("UA300", "UA", "United Airlines", lax, jfk, 
                baseDate.AddHours(12), baseDate.AddHours(19), 
                new Money(199.99m, "USD"), CabinClass.Economy),
            
            new Flight("AA101", "AA", "American Airlines", lax, jfk, 
                baseDate.AddHours(6), baseDate.AddHours(12.5), 
                new Money(499.99m, "USD"), CabinClass.Business),
            
            new Flight("DL201", "DL", "Delta Air Lines", lax, jfk, 
                baseDate.AddHours(14), baseDate.AddHours(20), 
                new Money(349.99m, "USD"), CabinClass.Economy)
        };

        // Add segments for each flight
        foreach (var flight in flights)
        {
            var segment = new FlightSegment(
                flight.FlightNumber,
                flight.AirlineCode,
                lax,
                jfk,
                flight.DepartureTime,
                flight.ArrivalTime,
                1); // segmentOrder
            
            flight.AddSegment(segment);
        }

        context.Flights.AddRange(flights);
        await context.SaveChangesAsync();

        return flights;
    }
}
