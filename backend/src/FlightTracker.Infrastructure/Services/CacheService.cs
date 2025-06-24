using FlightTracker.Domain.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace FlightTracker.Infrastructure.Services;

/// <summary>
/// In-memory cache service implementation
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;

    public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        await Task.CompletedTask; // Make it async for interface consistency
        
        if (_memoryCache.TryGetValue(key, out var value) && value is T typedValue)
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return typedValue;
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        return default;
    }    public async Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken cancellationToken = default) where T : class
    {
        await Task.CompletedTask; // Make it async for interface consistency

        var options = new MemoryCacheEntryOptions();
        options.SetAbsoluteExpiration(expiry);

        _memoryCache.Set(key, value, options);
        _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", 
            key, expiry.ToString());
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // Make it async for interface consistency
        
        _memoryCache.Remove(key);
        _logger.LogDebug("Removed cache entry for key: {Key}", key);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask; // Make it async for interface consistency
        
        return _memoryCache.TryGetValue(key, out _);
    }
}
