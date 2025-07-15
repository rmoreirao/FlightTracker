using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Repositories;
using FlightTracker.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of flight query repository
/// </summary>
public class EfFlightQueryRepository : EfBaseRepository<FlightQuery, Guid>, IFlightQueryRepository
{
    public EfFlightQueryRepository(
        FlightDbContext context,
        ILogger<EfFlightQueryRepository> logger)
        : base(context, logger)
    {
    }

    public override async Task<FlightQuery?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Include(fq => fq.Origin)
                .Include(fq => fq.Destination)
                .AsNoTracking()
                .FirstOrDefaultAsync(fq => fq.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flight query by ID {Id}", id);
            throw;
        }
    }

    public override async Task<FlightQuery> AddAsync(FlightQuery flightQuery, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await base.AddAsync(flightQuery, cancellationToken);
            _logger.LogInformation("Added flight query {QueryId} for route {Origin}-{Destination}",
                flightQuery.Id, flightQuery.OriginCode, flightQuery.DestinationCode);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding flight query for route {Origin}-{Destination}",
                flightQuery.OriginCode, flightQuery.DestinationCode);
            throw;
        }
    }

    public override async Task<FlightQuery> UpdateAsync(FlightQuery flightQuery, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await base.UpdateAsync(flightQuery, cancellationToken);
            _logger.LogInformation("Updated flight query {QueryId}", flightQuery.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating flight query {QueryId}", flightQuery.Id);
            throw;
        }
    }

    public async Task<IReadOnlyList<FlightQuery>> GetByUserAsync(
        string userId,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queries = await _dbSet
                .Include(fq => fq.Origin)
                .Include(fq => fq.Destination)
                .Where(fq => fq.UserId == userId)
                .OrderByDescending(fq => fq.LastSearchedAt)
                .Skip(skip)
                .Take(take)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {Count} flight queries for user {UserId}", queries.Count, userId);
            return queries.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flight queries for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IReadOnlyList<FlightQuery>> GetPopularQueriesAsync(
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var popularQueries = await _dbSet
                .Include(fq => fq.Origin)
                .Include(fq => fq.Destination)
                .OrderByDescending(fq => fq.SearchCount)
                .ThenByDescending(fq => fq.LastSearchedAt)
                .Take(count)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {Count} popular flight queries", popularQueries.Count);
            return popularQueries.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular flight queries");
            throw;
        }
    }

    public async Task<FlightQuery?> FindExistingQueryAsync(
        string originCode,
        string destinationCode,
        DateTime departureDate,
        DateTime? returnDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Include(fq => fq.Origin)
                .Include(fq => fq.Destination)
                .AsNoTracking()
                .FirstOrDefaultAsync(fq =>
                    fq.OriginCode == originCode &&
                    fq.DestinationCode == destinationCode &&
                    fq.DepartureDate.Date == departureDate.Date &&
                    fq.ReturnDate == returnDate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding existing query for route {Origin}-{Destination}",
                originCode, destinationCode);
            throw;
        }
    }

    public async Task<FlightQuery> IncrementSearchCountAsync(Guid queryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = await _dbSet.FirstOrDefaultAsync(fq => fq.Id == queryId, cancellationToken);
            if (query == null)
                throw new InvalidOperationException($"Flight query with ID {queryId} not found");

            query.IncrementSearchCount();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Incremented search count for query {QueryId} to {SearchCount}",
                queryId, query.SearchCount);
            return query;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing search count for query {QueryId}", queryId);
            throw;
        }
    }

    public async Task DeleteOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        try
        {
            var oldQueries = await _dbSet
                .Where(fq => fq.CreatedAt < cutoffDate)
                .ToListAsync(cancellationToken);

            if (oldQueries.Any())
            {
                _dbSet.RemoveRange(oldQueries);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Deleted {Count} flight queries older than {CutoffDate}",
                    oldQueries.Count, cutoffDate);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting old flight queries older than {CutoffDate}", cutoffDate);
            throw;
        }
    }

    public async Task<IReadOnlyList<FlightQuery>> GetRecentQueriesAsync(
        TimeSpan timeWindow,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow - timeWindow;
            var query = _dbSet
                .Include(fq => fq.Origin)
                .Include(fq => fq.Destination)
                .Where(fq => fq.LastSearchedAt >= cutoffTime)
                .OrderByDescending(fq => fq.LastSearchedAt)
                .AsNoTracking();

            if (limit.HasValue)
            {
                query = query.Take(limit.Value);
            }

            var recentQueries = await query.ToListAsync(cancellationToken);

            _logger.LogDebug("Retrieved {Count} recent flight queries within {TimeWindow}",
                recentQueries.Count, timeWindow);
            return recentQueries.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent flight queries within {TimeWindow}", timeWindow);
            throw;
        }
    }
}
