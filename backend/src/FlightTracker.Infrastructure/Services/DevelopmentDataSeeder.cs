using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Enums;
using FlightTracker.Domain.ValueObjects;
using FlightTracker.Infrastructure.Configuration;

namespace FlightTracker.Infrastructure.Services;

/// <summary>
/// Implementation of development data seeder with realistic test data
/// </summary>
public class DevelopmentDataSeeder : IDevelopmentDataSeeder
{
    private readonly FlightDbContext _context;
    private readonly ILogger<DevelopmentDataSeeder> _logger;
    private readonly DatabaseOptions _options;
    private readonly Random _random;

    public DevelopmentDataSeeder(
        FlightDbContext context, 
        ILogger<DevelopmentDataSeeder> logger,
        IOptions<DatabaseOptions> options)
    {
        _context = context;
        _logger = logger;
        _options = options.Value;
        _random = new Random(42); // Consistent seed for reproducible data
    }

    public async Task SeedAsync()
    {
        // Check if we should force reseed by deleting all data first
        if (_options.ForceReseedData)
        {
            _logger.LogInformation("🗑️ Force reseed enabled - deleting all existing data...");
            await DeleteAllDataAsync();
        }
        else if (await _context.Airports.AnyAsync())
        {
            _logger.LogInformation("📝 Test data already exists, skipping seed");
            return;
        }

        _logger.LogInformation("🌱 Seeding development test data...");
        
        await SeedAirportsAsync();
        await SeedAirlinesAsync();
        await SeedFlightsAsync();
    await SeedItinerariesAsync();
        await SeedFlightQueriesAsync();
        await SeedPriceHistoryAsync();
        
        _logger.LogInformation("✅ Test data seeding completed");
    }

    private async Task DeleteAllDataAsync()
    {
        _logger.LogInformation("🗑️ Deleting all existing data in correct order...");

        // Delete in reverse dependency order to respect foreign key constraints
        // (ItineraryLegs -> Itineraries depend on Flights)
        var itineraryLegsCount = await _context.ItineraryLegs.CountAsync();
        if (itineraryLegsCount > 0)
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"ItineraryLegs\"");
            _logger.LogInformation("🗑️ Deleted {Count} itinerary legs", itineraryLegsCount);
        }

        var itinerariesCount = await _context.Itineraries.CountAsync();
        if (itinerariesCount > 0)
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"Itineraries\"");
            _logger.LogInformation("🗑️ Deleted {Count} itineraries", itinerariesCount);
        }
        
        // 1. Delete PriceSnapshots (has FKs to FlightQueries and Airlines)
        var priceSnapshotsCount = await _context.PriceSnapshots.CountAsync();
        if (priceSnapshotsCount > 0)
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"PriceSnapshots\"");
            _logger.LogInformation("🗑️ Deleted {Count} price snapshots", priceSnapshotsCount);
        }

        // 2. Delete FlightSegments (has FK to Flights)
        var segmentsCount = await _context.FlightSegments.CountAsync();
        if (segmentsCount > 0)
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"FlightSegments\"");
            _logger.LogInformation("🗑️ Deleted {Count} flight segments", segmentsCount);
        }

        // 3. Delete Flights (has FKs to Airlines and Airports)
        var flightsCount = await _context.Flights.CountAsync();
        if (flightsCount > 0)
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"Flights\"");
            _logger.LogInformation("🗑️ Deleted {Count} flights", flightsCount);
        }

        // 4. Delete FlightQueries (has FKs to Airports)
        var queriesCount = await _context.FlightQueries.CountAsync();
        if (queriesCount > 0)
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"FlightQueries\"");
            _logger.LogInformation("🗑️ Deleted {Count} flight queries", queriesCount);
        }

        // 5. Delete Airlines (no dependencies on it now)
        var airlinesCount = await _context.Airlines.CountAsync();
        if (airlinesCount > 0)
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"Airlines\"");
            _logger.LogInformation("🗑️ Deleted {Count} airlines", airlinesCount);
        }

        // 6. Delete Airports (no dependencies on it now)
        var airportsCount = await _context.Airports.CountAsync();
        if (airportsCount > 0)
        {
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"Airports\"");
            _logger.LogInformation("🗑️ Deleted {Count} airports", airportsCount);
        }

        _logger.LogInformation("✅ All existing data deleted successfully");
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
            new Airport("PDX", "Portland International", "Portland", "United States", 45.5898m, -122.5951m),
            new Airport("AMS", "Amsterdam Airport Schiphol", "Amsterdam", "Netherlands", 52.3105m, 4.7683m),
            new Airport("GIG", "Rio de Janeiro-Galeão International Airport", "Rio de Janeiro", "Brazil", -22.8099m, -43.2505m)
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
            ("MSP", "DTW"), ("DTW", "MSP"),
            ("JFK", "AMS"), ("AMS", "JFK"),
            ("LAX", "AMS"), ("AMS", "LAX"),
            ("MIA", "GIG"), ("GIG", "MIA"),
            ("JFK", "GIG"), ("GIG", "JFK"),
            ("AMS", "GIG"), ("GIG", "AMS")
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
            var date = DateTime.UtcNow.Date.AddDays(day);
            
            // 3-5 flights per day on this route
            var flightCount = _random.Next(3, 6);

            for (int i = 0; i < flightCount; i++)
            {
                var airline = airlines[_random.Next(airlines.Count)];
                var departureTime = DateTime.SpecifyKind(date.AddHours(6 + (i * 4) + _random.Next(-60, 60)), DateTimeKind.Utc); // Spread throughout day
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

                flight.AddSegment(
                    new FlightSegment(
                        flightNumber,
                        airline.Code,
                        originAirport,
                        destinationAirport,
                        departureTime,
                        arrivalTime,
                        1,
                        null,
                        FlightStatus.Scheduled
                        )
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
            ("PHX", "MSP"), ("DTW", "PDX"),
            ("JFK", "AMS"), ("AMS", "JFK"),
            ("LAX", "AMS"), ("MIA", "GIG"), 
            ("JFK", "GIG"), ("AMS", "GIG")
        };

        foreach (var (origin, destination) in routes)
        {
            // One-way queries
            for (int i = 0; i < 5; i++)
            {
                var departureDate = DateTime.UtcNow.Date.AddDays(i * 3 + 1);
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
                var departureDate = DateTime.UtcNow.Date.AddDays(i * 7 + 3);
                var returnDate = departureDate.AddDays(3 + i);
                var query = new FlightQuery(origin, destination, departureDate, returnDate);
                _context.FlightQueries.Add(query);
            }
        }
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("🔍 Seeded flight queries for {Count} routes", routes.Length);
    }

    private async Task SeedItinerariesAsync()
    {
        if (!_options.GenerateItineraries)
        {
            _logger.LogInformation("🛑 Itinerary generation disabled via config");
            return;
        }

        // Avoid duplicate generation if already present (in non-force reseed scenarios)
        if (!_options.ForceReseedData && await _context.Itineraries.AnyAsync())
        {
            _logger.LogInformation("ℹ️ Itineraries already exist, skipping generation");
            return;
        }

        _logger.LogInformation("🧪 Generating itineraries (one-way & round-trip)...");

        // Load flights with necessary navigation (segments already loaded by include for search, but here we create legs directly)
        var flights = await _context.Flights
            .Include(f => f.Segments)
            .AsNoTracking()
            .ToListAsync();

        if (flights.Count == 0)
        {
            _logger.LogWarning("No flights available for itinerary generation");
            return;
        }

        // Group flights by (origin,destination, departureDate)
        var grouped = flights
            .Select(f => new
            {
                Flight = f,
                Origin = f.Segments.First().OriginCode,
                Destination = f.Segments.Last().DestinationCode,
                Date = f.Segments.First().DepartureTime.Date
            })
            .GroupBy(x => (x.Origin, x.Destination, x.Date))
            .ToDictionary(g => g.Key, g => g.Select(x => x.Flight).OrderBy(f => f.DepartureTime).ToList());

        var oneWayCap = _options.MaxOneWayPerRoutePerDay;
        var roundTripCap = _options.MaxRoundTripsPerRoutePerDay;
        var minReturn = _options.MinReturnTripDays;
        var maxReturn = _options.MaxReturnTripDays;

        var newItineraries = new List<Itinerary>();
        var random = _random; // reuse seeded RNG

        // One-way itineraries
        foreach (var kvp in grouped)
        {
            var flightsForKey = kvp.Value;
            var sample = flightsForKey.Take(oneWayCap).ToList();
            foreach (var flight in sample)
            {
                var seg = flight.Segments.First();
                var leg = new ItineraryLeg(
                    sequence: 0,
                    flightId: flight.Id,
                    flightNumber: flight.FlightNumber,
                    airlineCode: flight.AirlineCode,
                    originCode: seg.OriginCode,
                    destinationCode: seg.DestinationCode,
                    departureUtc: seg.DepartureTime,
                    arrivalUtc: seg.ArrivalTime,
                    // Clone Money to avoid sharing owned instance across multiple legs
                    priceComponent: new Money(flight.Price.Amount, flight.Price.Currency),
                    cabinClass: flight.CabinClass,
                    direction: LegDirection.Outbound);
                var itin = Itinerary.Create(new[] { leg });
                newItineraries.Add(itin);
            }
        }

        // Round-trip itineraries: pair routes (A->B) with (B->A)
        var roundTripCount = 0;
        foreach (var kvp in grouped)
        {
            var (orig, dest, date) = kvp.Key;
            var outboundFlights = kvp.Value;
            // For each potential return date window
            for (int days = minReturn; days <= maxReturn; days++)
            {
                var returnDate = date.AddDays(days);
                if (!grouped.TryGetValue((dest, orig, returnDate), out var returnFlights))
                    continue;

                foreach (var outbound in outboundFlights.Take(roundTripCap))
                {
                    if (roundTripCount >= roundTripCap) break; // cap per departure day / route
                    var ret = returnFlights.FirstOrDefault();
                    if (ret == null) continue;

                    var outSeg = outbound.Segments.First();
                    var retSeg = ret.Segments.First();

                    // Currency consistency check
                    if (outbound.Price.Currency != ret.Price.Currency) continue;

                    var leg0 = new ItineraryLeg(0, outbound.Id, outbound.FlightNumber, outbound.AirlineCode,
                        outSeg.OriginCode, outSeg.DestinationCode, outSeg.DepartureTime, outSeg.ArrivalTime, new Money(outbound.Price.Amount, outbound.Price.Currency), outbound.CabinClass, LegDirection.Outbound);
                    var leg1 = new ItineraryLeg(1, ret.Id, ret.FlightNumber, ret.AirlineCode,
                        retSeg.OriginCode, retSeg.DestinationCode, retSeg.DepartureTime, retSeg.ArrivalTime, new Money(ret.Price.Amount, ret.Price.Currency), ret.CabinClass, LegDirection.Return);
                    var itin = Itinerary.Create(new[] { leg0, leg1 });
                    newItineraries.Add(itin);
                    roundTripCount++;
                }
                if (roundTripCount >= roundTripCap) break;
            }
            roundTripCount = 0; // reset for next outbound route-date
        }

        // Persist in batches
        const int batchSize = 200;
        for (int i = 0; i < newItineraries.Count; i += batchSize)
        {
            var batch = newItineraries.Skip(i).Take(batchSize).ToList();
            _context.Itineraries.AddRange(batch);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("🧾 Generated {OneWay} one-way and {RoundTrip} round-trip itineraries (total {Total})", 
            newItineraries.Count(i => i.Legs.Count == 1),
            newItineraries.Count(i => i.Legs.Count == 2),
            newItineraries.Count);
    }

    // Internal test hook (avoid making main method public)
    public Task SeedItinerariesAsyncWrapperForTest() => SeedItinerariesAsync();

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
