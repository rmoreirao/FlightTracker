using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Enums;
using FlightTracker.Domain.Repositories;
using FlightTracker.Domain.ValueObjects;
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
        FlightSearchOptions? searchOptions = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure DateTime values are in UTC for PostgreSQL compatibility
            var departureDateUtc = departureDate.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(departureDate, DateTimeKind.Utc) 
                : departureDate.ToUniversalTime();

            var returnDateUtc = returnDate?.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(returnDate.Value, DateTimeKind.Utc) 
                : returnDate?.ToUniversalTime();

            // Search for outbound flights
            var outboundQuery = _dbSet
                .Include(f => f.Segments)
                    .ThenInclude(s => s.Origin)
                .Include(f => f.Segments)
                    .ThenInclude(s => s.Destination)
                .Include(f => f.Segments)
                    .ThenInclude(s => s.Airline)
                .Include(f => f.Origin)
                .Include(f => f.Destination)
                .Where(f => f.Segments.Any(s => 
                    s.OriginCode == originCode && 
                    s.DestinationCode == destinationCode &&
                    s.DepartureTime.Date == departureDateUtc.Date))
                .AsNoTracking();

            var allFlights = new List<Flight>();

            // Apply sorting and pagination to outbound flights
            var outboundQueryWithOptions = outboundQuery;
            if (searchOptions?.SortBy != null)
            {
                outboundQueryWithOptions = ApplySorting(outboundQueryWithOptions, searchOptions.SortBy, searchOptions.SortOrder);
            }
            else
            {
                outboundQueryWithOptions = outboundQueryWithOptions.OrderBy(f => f.Segments.Min(s => s.DepartureTime));
            }

            // Apply pagination - for round-trip, we need to consider both directions
            var pageSize = searchOptions?.PageSize ?? 100;
            var skip = searchOptions?.Page > 0 ? (searchOptions.Page - 1) * pageSize : 0;
            
            if (returnDateUtc.HasValue)
            {
                // For round-trip searches, get half the page size for each direction
                var halfPageSize = Math.Max(1, pageSize / 2);
                outboundQueryWithOptions = outboundQueryWithOptions.Skip(skip / 2).Take(halfPageSize);
                
                var outboundFlights = await outboundQueryWithOptions.ToListAsync(cancellationToken);
                allFlights.AddRange(outboundFlights);

                // Search for return flights (swap origin and destination)
                var returnQuery = _dbSet
                    .Include(f => f.Segments)
                        .ThenInclude(s => s.Origin)
                    .Include(f => f.Segments)
                        .ThenInclude(s => s.Destination)
                    .Include(f => f.Segments)
                        .ThenInclude(s => s.Airline)
                    .Include(f => f.Origin)
                    .Include(f => f.Destination)
                    .Where(f => f.Segments.Any(s => 
                        s.OriginCode == destinationCode && 
                        s.DestinationCode == originCode &&
                        s.DepartureTime.Date == returnDateUtc.Value.Date))
                    .AsNoTracking();

                // Apply same sorting logic to return flights
                if (searchOptions?.SortBy != null)
                {
                    returnQuery = ApplySorting(returnQuery, searchOptions.SortBy, searchOptions.SortOrder);
                }
                else
                {
                    returnQuery = returnQuery.OrderBy(f => f.Segments.Min(s => s.DepartureTime));
                }

                returnQuery = returnQuery.Skip(skip / 2).Take(halfPageSize);
                var returnFlights = await returnQuery.ToListAsync(cancellationToken);
                allFlights.AddRange(returnFlights);

                _logger.LogDebug("Round-trip flight search for {Origin}-{Destination} on {DepartureDate} returning {ReturnDate}: found {OutboundCount} outbound and {ReturnCount} return flights",
                    originCode, destinationCode, departureDateUtc, returnDateUtc, outboundFlights.Count, returnFlights.Count);
            }
            else
            {
                // One-way search
                outboundQueryWithOptions = outboundQueryWithOptions.Skip(skip).Take(pageSize);
                var outboundFlights = await outboundQueryWithOptions.ToListAsync(cancellationToken);
                allFlights.AddRange(outboundFlights);

                _logger.LogDebug("One-way flight search for {Origin}-{Destination} on {DepartureDate} returned {Count} results",
                    originCode, destinationCode, departureDateUtc, outboundFlights.Count);
            }

            return allFlights.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching flights for route {Origin}-{Destination}",
                originCode, destinationCode);
            throw;
        }
    }

    private IQueryable<Flight> ApplySorting(IQueryable<Flight> query, FlightSortBy sortBy, SortOrder sortOrder)
    {
        return sortBy switch
        {
            FlightSortBy.DepartureTime => sortOrder == SortOrder.Ascending
                ? query.OrderBy(f => f.Segments.Min(s => s.DepartureTime))
                : query.OrderByDescending(f => f.Segments.Min(s => s.DepartureTime)),
            
            FlightSortBy.ArrivalTime => sortOrder == SortOrder.Ascending
                ? query.OrderBy(f => f.Segments.Max(s => s.ArrivalTime))
                : query.OrderByDescending(f => f.Segments.Max(s => s.ArrivalTime)),
            
            FlightSortBy.Duration => sortOrder == SortOrder.Ascending
                ? query.OrderBy(f => f.Duration)
                : query.OrderByDescending(f => f.Duration),
            
            FlightSortBy.Price => sortOrder == SortOrder.Ascending
                ? query.OrderBy(f => f.Price.Amount)
                : query.OrderByDescending(f => f.Price.Amount),
            
            FlightSortBy.Airline => sortOrder == SortOrder.Ascending
                ? query.OrderBy(f => f.Segments.First().AirlineCode)
                : query.OrderByDescending(f => f.Segments.First().AirlineCode),
            
            _ => query.OrderBy(f => f.Segments.Min(s => s.DepartureTime))
        };
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
            // Ensure DateTime is in UTC for PostgreSQL compatibility
            var departureDateUtc = departureDate.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(departureDate, DateTimeKind.Utc) 
                : departureDate.ToUniversalTime();

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
                    s.DepartureTime.Date == departureDateUtc.Date), cancellationToken);
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
