using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Enums;
using FlightTracker.Domain.Repositories;
using FlightTracker.Domain.ValueObjects;

namespace FlightTracker.Infrastructure.Repositories;

public class MockFlightRepository : IFlightRepository
{
    private readonly List<Flight> _flights;
    private readonly IAirportRepository _airportRepository;

    public MockFlightRepository(IAirportRepository airportRepository)
    {
        _airportRepository = airportRepository;
        _flights = new List<Flight>();
        _ = InitializeFlights(); // Fire and forget initialization
    }

    public Task<IReadOnlyList<Flight>> SearchAsync(
        string originCode,
        string destinationCode,
        DateTime departureDate,
        DateTime? returnDate = null,
        CancellationToken cancellationToken = default)
    {
        // var results = _flights.Where(f =>
        //     f.Origin.Code == originCode &&
        //     f.Destination.Code == destinationCode &&
        //     f.DepartureTime.Date == departureDate.Date)
        //     .ToList();

        var results = _flights.ToList();
        
        return Task.FromResult<IReadOnlyList<Flight>>(results);
    }

    public Task<Flight?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Since Flight doesn't have an Id property, we'll use FlightNumber as identifier
        // This is a mock implementation limitation
        return Task.FromResult<Flight?>(null);
    }

    public Task<Flight?> GetByFlightNumberAsync(
        string flightNumber,
        string airlineCode,
        DateTime departureDate,
        CancellationToken cancellationToken = default)
    {
        var flight = _flights.FirstOrDefault(f =>
            f.FlightNumber == flightNumber &&
            f.AirlineCode == airlineCode &&
            f.DepartureTime.Date == departureDate.Date);

        return Task.FromResult(flight);
    }

    public Task<Flight> AddAsync(Flight flight, CancellationToken cancellationToken = default)
    {
        _flights.Add(flight);
        return Task.FromResult(flight);
    }

    public Task<Flight> UpdateAsync(Flight flight, CancellationToken cancellationToken = default)
    {
        var existingIndex = _flights.FindIndex(f =>
            f.FlightNumber == flight.FlightNumber &&
            f.AirlineCode == flight.AirlineCode &&
            f.DepartureTime.Date == flight.DepartureTime.Date);

        if (existingIndex >= 0)
        {
            _flights[existingIndex] = flight;
        }
        return Task.FromResult(flight);
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Since Flight doesn't have an Id property, this is a no-op in mock
        return Task.CompletedTask;
    }

    private async Task InitializeFlights()
    {
        try
        {
            var airports = await _airportRepository.GetAllAsync();
            var airportList = airports.ToList();

            if (airportList.Count < 2)
                return;

            var random = new Random(42); // Fixed seed for consistent data
            var airlines = new[] { "AA", "DL", "UA", "WN", "B6" };
            var airlineNames = new Dictionary<string, string>
            {
                { "AA", "American Airlines" },
                { "DL", "Delta Air Lines" },
                { "UA", "United Airlines" },
                { "WN", "Southwest Airlines" },
                { "B6", "JetBlue Airways" }
            };

            for (int i = 0; i < 100; i++)
            {
                var originAirport = airportList[random.Next(airportList.Count)];
                var destinationAirport = airportList[random.Next(airportList.Count)];
                
                // Ensure origin and destination are different
                while (destinationAirport.Code == originAirport.Code)
                {
                    destinationAirport = airportList[random.Next(airportList.Count)];
                }

                var airlineCode = airlines[random.Next(airlines.Length)];
                var flightNumber = $"{airlineCode}{random.Next(1000, 9999)}";
                
                var baseDate = DateTime.Today.AddDays(random.Next(1, 90));
                var departureTime = baseDate.AddHours(random.Next(6, 22)).AddMinutes(random.Next(0, 4) * 15);
                var duration = TimeSpan.FromMinutes(random.Next(90, 360));
                var arrivalTime = departureTime.Add(duration);
                
                var price = new Money(random.Next(200, 1200), "USD");
                var cabinClass = (CabinClass)random.Next(1, 5);

                var segment = new FlightSegment(
                    flightNumber,
                    airlineCode,
                    originAirport,
                    destinationAirport,
                    departureTime,
                    arrivalTime,
                    1, // segment order
                    $"Boeing {random.Next(700, 800)}" // aircraft type
                );

                var flight = new Flight(
                    flightNumber,
                    airlineCode,
                    airlineNames[airlineCode],
                    originAirport,
                    destinationAirport,
                    departureTime,
                    arrivalTime,
                    price,
                    cabinClass
                );

                // Add the segment to the flight
                flight.AddSegment(segment);

                _flights.Add(flight);
            }
        }
        catch
        {
            // If initialization fails, just use empty flights list
        }
    }
}
