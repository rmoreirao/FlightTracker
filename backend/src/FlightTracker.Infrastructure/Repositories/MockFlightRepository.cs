using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Repositories;
using FlightTracker.Domain.ValueObjects;

namespace FlightTracker.Infrastructure.Repositories;

/// <summary>
/// Mock implementation of flight repository for testing purposes
/// </summary>
public class MockFlightRepository : IFlightRepository
{
    private readonly List<Flight> _flights = new();

    public Task<IReadOnlyList<Flight>> SearchAsync(
        string originCode,
        string destinationCode,
        DateTime departureDate,
        FlightSearchOptions? searchOptions = null,
        CancellationToken cancellationToken = default)
    {
        var flights = _flights
            .Where(f => f.Origin?.Code == originCode &&
                       f.Destination?.Code == destinationCode &&
                       f.DepartureTime.Date == departureDate.Date)
            .ToList();

        return Task.FromResult<IReadOnlyList<Flight>>(flights.AsReadOnly());
    }

    public Task<Flight?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var flight = _flights.FirstOrDefault(f => f.Id == id);
        return Task.FromResult(flight);
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
        var index = _flights.FindIndex(f => f.Id == flight.Id);
        if (index >= 0)
        {
            _flights[index] = flight;
        }
        return Task.FromResult(flight);
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var flight = _flights.FirstOrDefault(f => f.Id == id);
        if (flight != null)
        {
            _flights.Remove(flight);
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Flight>> GetByRouteAsync(
        string originCode,
        string destinationCode,
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.Date;
        var endDate = startDate.AddDays(days);

        var flights = _flights
            .Where(f => f.Origin?.Code == originCode &&
                       f.Destination?.Code == destinationCode &&
                       f.DepartureTime.Date >= startDate &&
                       f.DepartureTime.Date <= endDate)
            .ToList();

        return Task.FromResult<IReadOnlyList<Flight>>(flights.AsReadOnly());
    }

    public Task<IReadOnlyList<Flight>> GetRecentFlightsAsync(
        int count = 50,
        CancellationToken cancellationToken = default)
    {
        var flights = _flights
            .OrderByDescending(f => f.CreatedAt)
            .Take(count)
            .ToList();

        return Task.FromResult<IReadOnlyList<Flight>>(flights.AsReadOnly());
    }
}