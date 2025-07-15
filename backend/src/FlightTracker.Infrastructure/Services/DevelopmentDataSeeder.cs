using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Enums;
using FlightTracker.Domain.ValueObjects;

namespace FlightTracker.Infrastructure.Services;

/// <summary>
/// Implementation of development data seeder with realistic test data
/// </summary>
public class DevelopmentDataSeeder : IDevelopmentDataSeeder
{
    private readonly FlightDbContext _context;
    private readonly ILogger<DevelopmentDataSeeder> _logger;
    private readonly Random _random;

    public DevelopmentDataSeeder(FlightDbContext context, ILogger<DevelopmentDataSeeder> logger)
    {
        _context = context;
        _logger = logger;
        _random = new Random(42); // Consistent seed for reproducible data
    }

    public async Task SeedAsync()
    {
        if (await _context.Airports.AnyAsync())
        {
            _logger.LogInformation("📝 Test data already exists, skipping seed");
            return;
        }

        _logger.LogInformation("🌱 Seeding development test data...");
        
        await SeedAirportsAsync();
        await SeedAirlinesAsync();
        await SeedFlightsAsync();
        await SeedFlightQueriesAsync();
        await SeedPriceHistoryAsync();
        
        _logger.LogInformation("✅ Test data seeding completed");
    }

    public async Task SeedAirportsAsync()
    {
        var airports = new[]
        {
            new Airport("JFK", "John F. Kennedy International", "New York", "United States", 40.6413m, -73.7781m),
            new Airport("LAX", "Los Angeles International", "Los Angeles", "United States", 33.9425m, -118.4081m),
            new Airport("ORD", "O'Hare International", "Chicago", "United States", 41.9742m, -87.9073m),
            new Airport("DFW", "Dallas/Fort Worth International", "Dallas", "United States", 32.8998m, -97.0403m),
            new Airport("DEN", "Denver International", "Denver", "United States", 39.8561m, -104.6737m),
            new Airport("SFO", "San Francisco International", "San Francisco", "United States", 37.6213m, -122.3790m),
            new Airport("SEA", "Seattle-Tacoma International", "Seattle", "United States", 47.4502m, -122.3088m),
            new Airport("MIA", "Miami International", "Miami", "United States", 25.7959m, -80.2870m),
            new Airport("BOS", "Logan International", "Boston", "United States", 42.3656m, -71.0096m),
            new Airport("LAS", "McCarran International", "Las Vegas", "United States", 36.0840m, -115.1537m),
            new Airport("PHX", "Phoenix Sky Harbor International", "Phoenix", "United States", 33.4484m, -112.0740m),
            new Airport("ATL", "Hartsfield-Jackson Atlanta International", "Atlanta", "United States", 33.6407m, -84.4277m),
            new Airport("MSP", "Minneapolis-St. Paul International", "Minneapolis", "United States", 44.8820m, -93.2218m),
            new Airport("DTW", "Detroit Metropolitan Wayne County", "Detroit", "United States", 42.2162m, -83.3554m),
            new Airport("PDX", "Portland International", "Portland", "United States", 45.5898m, -122.5951m)
        };

        _context.Airports.AddRange(airports);
        await _context.SaveChangesAsync();
        _logger.LogInformation("✈️  Seeded {Count} airports", airports.Length);
    }

    public async Task SeedAirlinesAsync()
    {
        var airlines = new[]
        {
            new Airline("AA", "American Airlines"),
            new Airline("UA", "United Airlines"), 
            new Airline("DL", "Delta Air Lines"),
            new Airline("WN", "Southwest Airlines"),
            new Airline("B6", "JetBlue Airways"),
            new Airline("AS", "Alaska Airlines"),
            new Airline("F9", "Frontier Airlines"),
            new Airline("NK", "Spirit Airlines"),
            new Airline("G4", "Allegiant Air"),
            new Airline("SY", "Sun Country Airlines")
        };

        _context.Airlines.AddRange(airlines);
        await _context.SaveChangesAsync();
        _logger.LogInformation("🏢 Seeded {Count} airlines", airlines.Length);
    }

    public async Task SeedFlightsAsync()
    {
        // Generate realistic flight schedules for popular routes
        var routes = new[]
        {
            ("JFK", "LAX"), ("LAX", "JFK"),
            ("ORD", "DFW"), ("DFW", "ORD"),
            ("SFO", "SEA"), ("SEA", "SFO"),
            ("BOS", "MIA"), ("MIA", "BOS"),
            ("DEN", "PHX"), ("PHX", "DEN"),
            ("ATL", "LAS"), ("LAS", "ATL"),
            ("MSP", "DTW"), ("DTW", "MSP")
        };

        var airlines = await _context.Airlines.ToListAsync();
        var airports = await _context.Airports.ToListAsync();

        foreach (var (origin, destination) in routes)
        {
            await SeedFlightsForRoute(origin, destination, airlines, airports);
        }
        
        _logger.LogInformation("🛫 Seeded flights for {Count} routes", routes.Length);
    }

    private async Task SeedFlightsForRoute(string originCode, string destinationCode, 
        List<Airline> airlines, List<Airport> airports)
    {
        var originAirport = airports.First(a => a.Code == originCode);
        var destinationAirport = airports.First(a => a.Code == destinationCode);
        
        // Generate flights for next 30 days
        for (int day = 0; day < 30; day++)
        {
            var date = DateTime.Today.AddDays(day);
            
            // 3-5 flights per day on this route
            var flightCount = _random.Next(3, 6);
            
            for (int i = 0; i < flightCount; i++)
            {
                var airline = airlines[_random.Next(airlines.Count)];
                var departureTime = date.AddHours(6 + (i * 4) + _random.Next(-60, 60)); // Spread throughout day
                var flightDuration = TimeSpan.FromHours(2 + _random.NextDouble() * 4); // 2-6 hour flights
                var arrivalTime = departureTime.Add(flightDuration);
                
                var flightNumber = $"{airline.Code}{1000 + _random.Next(1, 9999)}";
                
                // Generate a realistic price
                var price = new Money(200 + (decimal)(_random.NextDouble() * 800), "USD");
                var cabinClass = _random.Next(10) < 8 ? CabinClass.Economy : CabinClass.Business; // 80% economy
                
                var flight = new Flight(
                    flightNumber,
                    airline.Code,
                    airline.Name,
                    originAirport,
                    destinationAirport,
                    departureTime,
                    arrivalTime,
                    price,
                    cabinClass
                );
                
                _context.Flights.Add(flight);
            }
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task SeedFlightQueriesAsync()
    {
        // Generate sample search queries for analytics
        var routes = new[]
        {
            ("JFK", "LAX"), ("LAX", "SFO"), ("ORD", "MIA"), 
            ("BOS", "SEA"), ("DFW", "DEN"), ("ATL", "LAS"),
            ("PHX", "MSP"), ("DTW", "PDX")
        };

        foreach (var (origin, destination) in routes)
        {
            // One-way queries
            for (int i = 0; i < 5; i++)
            {
                var departureDate = DateTime.Today.AddDays(i * 3 + 1);
                var query = new FlightQuery(origin, destination, departureDate);
                
                // Simulate multiple searches for popular queries
                var searchCount = _random.Next(1, 10);
                for (int j = 1; j < searchCount; j++)
                {
                    query.IncrementSearchCount();
                }
                
                _context.FlightQueries.Add(query);
            }
            
            // Round-trip queries  
            for (int i = 0; i < 3; i++)
            {
                var departureDate = DateTime.Today.AddDays(i * 7 + 3);
                var returnDate = departureDate.AddDays(3 + i);
                var query = new FlightQuery(origin, destination, departureDate, returnDate);
                _context.FlightQueries.Add(query);
            }
        }
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("🔍 Seeded flight queries for {Count} routes", routes.Length);
    }

    public async Task SeedPriceHistoryAsync()
    {
        var queries = await _context.FlightQueries.Take(20).ToListAsync();
        var airlines = await _context.Airlines.ToListAsync();
        
        foreach (var query in queries)
        {
            // Generate price history for past 14 days
            for (int day = 14; day >= 0; day--)
            {
                var snapshotDate = DateTime.UtcNow.AddDays(-day);
                var basePrice = 200 + _random.Next(50, 500);
                var priceVariation = _random.Next(-50, 100);
                var finalPrice = Math.Max(100, basePrice + priceVariation);
                
                var airline = airlines[_random.Next(airlines.Count)];
                var price = new Money((decimal)finalPrice, "USD");
                var cabinClass = _random.Next(10) < 8 ? CabinClass.Economy : CabinClass.Business;
                
                var snapshot = new PriceSnapshot(
                    query.Id,
                    airline.Code,
                    cabinClass,
                    price
                );
                
                _context.PriceSnapshots.Add(snapshot);
            }
        }
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("💰 Seeded price history data for {Count} queries", queries.Count);
    }
}
