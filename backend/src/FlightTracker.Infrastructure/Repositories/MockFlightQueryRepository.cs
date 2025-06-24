using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Infrastructure.Repositories;

/// <summary>
/// Mock implementation of flight query repository for development
/// </summary>
public class MockFlightQueryRepository : IFlightQueryRepository
{
    private readonly ILogger<MockFlightQueryRepository> _logger;
    private readonly List<FlightQuery> _flightQueries;

    public MockFlightQueryRepository(ILogger<MockFlightQueryRepository> logger)
    {
        _logger = logger;
        _flightQueries = [];
    }

    public async Task<FlightQuery?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken);
        return _flightQueries.FirstOrDefault(fq => fq.Id == id);
    }

    public async Task<FlightQuery> AddAsync(FlightQuery flightQuery, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken);
        _flightQueries.Add(flightQuery);        _logger.LogInformation("Added flight query {QueryId} for route {Origin}-{Destination}",
            flightQuery.Id, flightQuery.OriginCode, flightQuery.DestinationCode);
        return flightQuery;
    }

    public async Task<FlightQuery> UpdateAsync(FlightQuery flightQuery, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken);
        var existingIndex = _flightQueries.FindIndex(fq => fq.Id == flightQuery.Id);
        if (existingIndex >= 0)
        {
            _flightQueries[existingIndex] = flightQuery;
            _logger.LogInformation("Updated flight query {QueryId}", flightQuery.Id);
        }
        return flightQuery;
    }

    public async Task<IReadOnlyList<FlightQuery>> GetByUserAsync(
        string userId,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {        // Mock implementation: since FlightQuery doesn't have UserId, return empty
        // In real implementation, user tracking would be handled differently
        await Task.Delay(100, cancellationToken);
        return Array.Empty<FlightQuery>();
    }

    public async Task<IReadOnlyList<FlightQuery>> GetPopularQueriesAsync(
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);        // Group by route and count occurrences
        var popularRoutes = _flightQueries
            .GroupBy(fq => new { fq.OriginCode, fq.DestinationCode })
            .Select(g => new { Route = g.Key, Count = g.Count(), Latest = g.OrderByDescending(fq => fq.LastSearchedAt).First() })
            .OrderByDescending(x => x.Count)
            .Take(count)
            .Select(x => x.Latest)
            .ToList();

        return popularRoutes.AsReadOnly();
    }

    public async Task DeleteOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);

        var toRemove = _flightQueries.Where(fq => fq.CreatedAt < cutoffDate).ToList();
        foreach (var query in toRemove)
        {
            _flightQueries.Remove(query);
        }

        _logger.LogInformation("Deleted {Count} flight queries older than {CutoffDate}",
            toRemove.Count, cutoffDate);
    }
}
