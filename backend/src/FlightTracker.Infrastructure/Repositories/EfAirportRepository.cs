using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Repositories;
using FlightTracker.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of airport repository with caching
/// </summary>
public class EfAirportRepository : EfBaseRepository<Airport, string>, IAirportRepository
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(4); // Airports rarely change

    public EfAirportRepository(
        FlightDbContext context, 
        IMemoryCache cache,
        ILogger<EfAirportRepository> logger) 
        : base(context, logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<Airport?> GetByCodeAsync(string iataCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(iataCode))
            return null;

        var normalizedCode = iataCode.ToUpperInvariant();
        var cacheKey = $"airport:{normalizedCode}";

        // Try to get from cache first
        if (_cache.TryGetValue(cacheKey, out Airport? cachedAirport))
        {
            _logger.LogDebug("Retrieved airport {Code} from cache", normalizedCode);
            return cachedAirport;
        }

        try
        {
            var airport = await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Code == normalizedCode, cancellationToken);

            if (airport != null)
            {
                // Cache the result
                _cache.Set(cacheKey, airport, _cacheExpiration);
                _logger.LogDebug("Cached airport {Code} for {Expiration}", normalizedCode, _cacheExpiration);
            }

            return airport;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting airport by code {Code}", normalizedCode);
            throw;
        }
    }

    public async Task<IReadOnlyList<Airport>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Array.Empty<Airport>();

        try
        {
            var lowerSearchTerm = searchTerm.ToLowerInvariant();
            
            var airports = await _dbSet
                .AsNoTracking()
                .Where(a => 
                    EF.Functions.Like(a.Code.ToLower(), $"%{lowerSearchTerm}%") ||
                    EF.Functions.Like(a.Name.ToLower(), $"%{lowerSearchTerm}%") ||
                    EF.Functions.Like(a.City.ToLower(), $"%{lowerSearchTerm}%") ||
                    EF.Functions.Like(a.Country.ToLower(), $"%{lowerSearchTerm}%"))
                .OrderBy(a => a.Code)
                .Take(50) // Limit results for performance
                .ToListAsync(cancellationToken);

            _logger.LogDebug("Airport search for '{SearchTerm}' returned {Count} results", searchTerm, airports.Count);
            return airports.AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching airports with term '{SearchTerm}'", searchTerm);
            throw;
        }
    }

    public async Task<IReadOnlyList<Airport>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "airports:all";
        
        // Try to get from cache first
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<Airport>? cachedAirports))
        {
            _logger.LogDebug("Retrieved all airports from cache");
            return cachedAirports!;
        }

        try
        {
            var airports = await _dbSet
                .AsNoTracking()
                .OrderBy(a => a.Code)
                .ToListAsync(cancellationToken);

            var readOnlyAirports = airports.AsReadOnly();
            
            // Cache the result
            _cache.Set(cacheKey, readOnlyAirports, _cacheExpiration);
            _logger.LogDebug("Cached all {Count} airports for {Expiration}", airports.Count, _cacheExpiration);

            return readOnlyAirports;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all airports");
            throw;
        }
    }

    public override async Task<Airport> AddAsync(Airport airport, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await base.AddAsync(airport, cancellationToken);
            
            // Invalidate relevant cache entries
            InvalidateAirportCache(airport.Code);
            
            _logger.LogInformation("Added airport {Code} - {Name}", airport.Code, airport.Name);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding airport {Code}", airport.Code);
            throw;
        }
    }

    public override async Task<Airport> UpdateAsync(Airport airport, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await base.UpdateAsync(airport, cancellationToken);
            
            // Invalidate relevant cache entries
            InvalidateAirportCache(airport.Code);
            
            _logger.LogInformation("Updated airport {Code} - {Name}", airport.Code, airport.Name);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating airport {Code}", airport.Code);
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
            InvalidateAirportCache(normalizedCode);
            
            _logger.LogInformation("Deleted airport {Code}", normalizedCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting airport {Code}", iataCode);
            throw;
        }
    }

    private void InvalidateAirportCache(string airportCode)
    {
        var cacheKeys = new[]
        {
            $"airport:{airportCode.ToUpperInvariant()}",
            "airports:all"
        };

        foreach (var key in cacheKeys)
        {
            _cache.Remove(key);
        }

        _logger.LogDebug("Invalidated airport cache for {Code}", airportCode);
    }
}
