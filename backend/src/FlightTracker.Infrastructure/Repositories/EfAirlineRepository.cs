using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Repositories;
using FlightTracker.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of airline repository with caching
/// </summary>
public class EfAirlineRepository : EfBaseRepository<Airline, string>, IAirlineRepository
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(6); // Airlines rarely change

    public EfAirlineRepository(
        FlightDbContext context,
        IMemoryCache cache,
        ILogger<EfAirlineRepository> logger)
        : base(context, logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<Airline?> GetByCodeAsync(string iataCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(iataCode))
            return null;

        var normalizedCode = iataCode.ToUpperInvariant();
        var cacheKey = $"airline:{normalizedCode}";

        // Try to get from cache first
        if (_cache.TryGetValue(cacheKey, out Airline? cachedAirline))
        {
            _logger.LogDebug("Retrieved airline {Code} from cache", normalizedCode);
            return cachedAirline;
        }

        try
        {
            var airline = await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Code == normalizedCode, cancellationToken);

            if (airline != null)
            {
                // Cache the result
                _cache.Set(cacheKey, airline, _cacheExpiration);
                _logger.LogDebug("Cached airline {Code} for {Expiration}", normalizedCode, _cacheExpiration);
            }

            return airline;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting airline by code {Code}", normalizedCode);
            throw;
        }
    }

    public async Task<IReadOnlyList<Airline>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Array.Empty<Airline>();

        try
        {
            var lowerSearchTerm = searchTerm.ToLowerInvariant();

            var airlines = await _dbSet
                .AsNoTracking()
                .Where(a =>
                    EF.Functions.Like(a.Code.ToLower(), $"%{lowerSearchTerm}%") ||
                    EF.Functions.Like(a.Name.ToLower(), $"%{lowerSearchTerm}%"))
                .OrderBy(a => a.Code)
                .Take(50) // Limit results for performance
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Airline search for '{SearchTerm}' returned {Count} results", searchTerm, airlines.Count);
            return airlines.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching airlines with term '{SearchTerm}'", searchTerm);
            throw;
        }
    }

    public async Task<IReadOnlyList<Airline>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "airlines:all";

        // Try to get from cache first
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<Airline>? cachedAirlines))
        {
            _logger.LogDebug("Retrieved all airlines from cache");
            return cachedAirlines!;
        }

        try
        {
            var airlines = await _dbSet
                .AsNoTracking()
                .OrderBy(a => a.Code)
                .ToListAsync(cancellationToken);

            var readOnlyAirlines = airlines.AsReadOnly();

            // Cache the result
            _cache.Set(cacheKey, readOnlyAirlines, _cacheExpiration);
            _logger.LogDebug("Cached all {Count} airlines for {Expiration}", airlines.Count, _cacheExpiration);

            return readOnlyAirlines;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all airlines");
            throw;
        }
    }

    public async Task<IReadOnlyList<Airline>> GetActiveAirlinesAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "airlines:active";

        // Try to get from cache first
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<Airline>? cachedActiveAirlines))
        {
            _logger.LogDebug("Retrieved active airlines from cache");
            return cachedActiveAirlines!;
        }

        try
        {
            // Get airlines that have price snapshots in the last 30 days
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            
            var activeAirlines = await _dbSet
                .AsNoTracking()
                .Where(a => _context.PriceSnapshots
                    .Any(ps => ps.AirlineCode == a.Code && ps.CollectedAt >= cutoffDate))
                .OrderBy(a => a.Code)
                .ToListAsync(cancellationToken);

            var readOnlyActiveAirlines = activeAirlines.AsReadOnly();

            // Cache the result for shorter time since this is dynamic
            _cache.Set(cacheKey, readOnlyActiveAirlines, TimeSpan.FromHours(1));
            _logger.LogDebug("Cached {Count} active airlines", activeAirlines.Count);

            return readOnlyActiveAirlines;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active airlines");
            throw;
        }
    }

    public override async Task<Airline> AddAsync(Airline airline, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await base.AddAsync(airline, cancellationToken);

            // Invalidate relevant cache entries
            InvalidateAirlineCache(airline.Code);

            _logger.LogInformation("Added airline {Code} - {Name}", airline.Code, airline.Name);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding airline {Code}", airline.Code);
            throw;
        }
    }

    public override async Task<Airline> UpdateAsync(Airline airline, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await base.UpdateAsync(airline, cancellationToken);

            // Invalidate relevant cache entries
            InvalidateAirlineCache(airline.Code);

            _logger.LogInformation("Updated airline {Code} - {Name}", airline.Code, airline.Name);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating airline {Code}", airline.Code);
            throw;
        }
    }

    public override async Task DeleteAsync(string iataCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(iataCode))
            return;

        try
        {
            var normalizedCode = iataCode.ToUpperInvariant();
            await base.DeleteAsync(normalizedCode, cancellationToken);

            // Invalidate relevant cache entries
            InvalidateAirlineCache(normalizedCode);

            _logger.LogInformation("Deleted airline {Code}", normalizedCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting airline {Code}", iataCode);
            throw;
        }
    }

    private void InvalidateAirlineCache(string airlineCode)
    {
        var cacheKeys = new[]
        {
            $"airline:{airlineCode.ToUpperInvariant()}",
            "airlines:all",
            "airlines:active"
        };

        foreach (var key in cacheKeys)
        {
            _cache.Remove(key);
        }

        _logger.LogDebug("Invalidated airline cache for {Code}", airlineCode);
    }
}
