using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Repositories;
using FlightTracker.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of flight repository
/// </summary>
public class EfFlightRepository : EfBaseRepository<Flight, Guid>, IFlightRepository
{
    public EfFlightRepository(
        FlightDbContext context,
        ILogger<EfFlightRepository> logger)
        : base(context, logger)
    {
    }

    public async Task<IReadOnlyList<Flight>> SearchAsync(
        string originCode,
        string destinationCode,
        DateTime departureDate,
        DateTime? returnDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet
                .Include(f => f.Segments)
                    .ThenInclude(s => s.Origin)
                .Include(f => f.Segments)
                    .ThenInclude(s => s.Destination)
                .Include(f => f.Segments)
                    .ThenInclude(s => s.Airline)
                .Where(f => f.Segments.Any(s => 
                    s.OriginCode == originCode && 
                    s.DestinationCode == destinationCode &&
                    s.DepartureTime.Date == departureDate.Date))
                .AsNoTracking();

            if (returnDate.HasValue)
            {
                query = query.Where(f => f.Segments.Any(s => 
                    s.OriginCode == destinationCode && 
                    s.DestinationCode == originCode &&
                    s.DepartureTime.Date == returnDate.Value.Date));
            }

            var flights = await query
                .OrderBy(f => f.Segments.Min(s => s.DepartureTime))
                .Take(100) // Reasonable limit for performance
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Flight search for {Origin}-{Destination} on {DepartureDate} returned {Count} results",
                originCode, destinationCode, departureDate, flights.Count);

            return flights.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching flights for route {Origin}-{Destination}",
                originCode, destinationCode);
            throw;
        }
    }

    public override async Task<Flight?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Include(f => f.Segments)
                    .ThenInclude(s => s.Origin)
                .Include(f => f.Segments)
                    .ThenInclude(s => s.Destination)
                .Include(f => f.Segments)
                    .ThenInclude(s => s.Airline)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flight by ID {Id}", id);
            throw;
        }
    }

    public async Task<Flight?> GetByFlightNumberAsync(
        string flightNumber,
        string airlineCode,
        DateTime departureDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Include(f => f.Segments)
                    .ThenInclude(s => s.Origin)
                .Include(f => f.Segments)
                    .ThenInclude(s => s.Destination)
                .Include(f => f.Segments)
                    .ThenInclude(s => s.Airline)
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.Segments.Any(s => 
                    s.FlightNumber == flightNumber &&
                    s.AirlineCode == airlineCode &&
                    s.DepartureTime.Date == departureDate.Date), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flight by number {FlightNumber} for airline {AirlineCode}",
                flightNumber, airlineCode);
            throw;
        }
    }

    public override async Task<Flight> AddAsync(Flight flight, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await base.AddAsync(flight, cancellationToken);
            _logger.LogInformation("Added flight {FlightId} with {SegmentCount} segments",
                flight.Id, flight.Segments.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding flight");
            throw;
        }
    }

    public override async Task<Flight> UpdateAsync(Flight flight, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await base.UpdateAsync(flight, cancellationToken);
            _logger.LogInformation("Updated flight {FlightId}", flight.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating flight {FlightId}", flight.Id);
            throw;
        }
    }

    public async Task<IReadOnlyList<Flight>> GetByRouteAsync(
        string originCode,
        string destinationCode,
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var startDate = DateTime.UtcNow.Date;
            var endDate = startDate.AddDays(days);

            var flights = await _dbSet
                .Include(f => f.Segments)
                    .ThenInclude(s => s.Origin)
                .Include(f => f.Segments)
                    .ThenInclude(s => s.Destination)
                .Include(f => f.Segments)
                    .ThenInclude(s => s.Airline)
                .Where(f => f.Segments.Any(s => 
                    s.OriginCode == originCode && 
                    s.DestinationCode == destinationCode &&
                    s.DepartureTime.Date >= startDate &&
                    s.DepartureTime.Date <= endDate))
                .OrderBy(f => f.Segments.Min(s => s.DepartureTime))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {Count} flights for route {Origin}-{Destination} over {Days} days",
                flights.Count, originCode, destinationCode, days);

            return flights.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flights by route {Origin}-{Destination}",
                originCode, destinationCode);
            throw;
        }
    }

    public async Task<IReadOnlyList<Flight>> GetRecentFlightsAsync(
        int count = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var flights = await _dbSet
                .Include(f => f.Segments)
                    .ThenInclude(s => s.Origin)
                .Include(f => f.Segments)
                    .ThenInclude(s => s.Destination)
                .Include(f => f.Segments)
                    .ThenInclude(s => s.Airline)
                .OrderByDescending(f => f.CreatedAt)
                .Take(count)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {Count} recent flights", flights.Count);
            return flights.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent flights");
            throw;
        }
    }
}
