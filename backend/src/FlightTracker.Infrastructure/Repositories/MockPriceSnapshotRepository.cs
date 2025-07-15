using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Repositories;
using FlightTracker.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Infrastructure.Repositories;

/// <summary>
/// Mock implementation of price snapshot repository for development
/// </summary>
public class MockPriceSnapshotRepository : IPriceSnapshotRepository
{
    private readonly ILogger<MockPriceSnapshotRepository> _logger;
    private readonly List<PriceSnapshot> _priceSnapshots;

    public MockPriceSnapshotRepository(ILogger<MockPriceSnapshotRepository> logger)
    {
        _logger = logger;
        _priceSnapshots = [];
    }    public async Task<IReadOnlyList<PriceSnapshot>> GetByRouteAsync(
        string originCode,
        string destinationCode,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);

        // For mock implementation, return empty list since we don't have route information directly in PriceSnapshot
        _logger.LogInformation("Mock implementation - GetByRouteAsync returning empty list");
        return new List<PriceSnapshot>().AsReadOnly();
    }

    public async Task<IReadOnlyList<PriceSnapshot>> GetByFlightAsync(
        Guid flightId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);

        // For mock implementation, return empty list since FlightId is not a direct property
        _logger.LogInformation("Mock implementation - GetByFlightAsync returning empty list");
        return new List<PriceSnapshot>().AsReadOnly();
    }

    public async Task<PriceSnapshot> AddAsync(PriceSnapshot priceSnapshot, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken);
        _priceSnapshots.Add(priceSnapshot);
        _logger.LogInformation("Added price snapshot for query {QueryId}", priceSnapshot.QueryId);
        return priceSnapshot;
    }

    public async Task<IReadOnlyList<PriceSnapshot>> AddRangeAsync(
        IEnumerable<PriceSnapshot> priceSnapshots,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        var snapshotList = priceSnapshots.ToList();
        _priceSnapshots.AddRange(snapshotList);
        _logger.LogInformation("Added {Count} price snapshots", snapshotList.Count);
        return snapshotList.AsReadOnly();
    }

    public async Task<PriceSnapshot?> GetLatestByFlightAsync(Guid flightId, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken);

        // For mock implementation, return null since FlightId is not a direct property
        _logger.LogInformation("Mock implementation - GetLatestByFlightAsync returning null");
        return null;
    }

    public async Task DeleteOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);

        var toRemove = _priceSnapshots.Where(ps => ps.CollectedAt < cutoffDate).ToList();
        foreach (var snapshot in toRemove)
        {
            _priceSnapshots.Remove(snapshot);
        }

        _logger.LogInformation("Deleted {Count} price snapshots older than {CutoffDate}",
            toRemove.Count, cutoffDate);
    }

    public async Task<IReadOnlyList<Domain.ValueObjects.PriceTrendData>> GetPriceTrendsAsync(
        string originCode,
        string destinationCode,
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);

        // Mock implementation - return some sample data
        var mockTrends = new List<Domain.ValueObjects.PriceTrendData>();
        var random = new Random(42);
        var basePrice = random.Next(300, 800);

        for (int i = 0; i < Math.Min(days, 7); i++)
        {
            var date = DateTime.UtcNow.Date.AddDays(-i);
            var variance = random.Next(-100, 100);
            var minPrice = new Money(basePrice + variance - 50, "USD");
            var maxPrice = new Money(basePrice + variance + 100, "USD");
            var avgPrice = new Money(basePrice + variance, "USD");
            var medianPrice = new Money(basePrice + variance + 10, "USD");

            mockTrends.Add(new Domain.ValueObjects.PriceTrendData(
                date, minPrice, maxPrice, avgPrice, medianPrice, random.Next(10, 50)));
        }

        _logger.LogInformation("Mock implementation - GetPriceTrendsAsync returning {Count} trends", mockTrends.Count);
        return mockTrends.AsReadOnly();
    }

    public async Task<IReadOnlyList<Domain.ValueObjects.AirlinePerformance>> GetAirlinePerformanceAsync(
        string originCode,
        string destinationCode,
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);

        // Mock implementation - return some sample data
        var mockPerformance = new List<Domain.ValueObjects.AirlinePerformance>();
        var airlines = new[] { ("AA", "American Airlines"), ("DL", "Delta"), ("UA", "United") };
        var random = new Random(42);

        foreach (var (code, name) in airlines)
        {
            var basePrice = random.Next(400, 900);
            var avgPrice = new Money(basePrice, "USD");
            var minPrice = new Money(basePrice - 100, "USD");
            var maxPrice = new Money(basePrice + 200, "USD");

            mockPerformance.Add(new Domain.ValueObjects.AirlinePerformance(
                code, name, avgPrice, minPrice, maxPrice, 
                random.Next(50, 200), random.NextDouble() * 2));
        }

        _logger.LogInformation("Mock implementation - GetAirlinePerformanceAsync returning {Count} performances", mockPerformance.Count);
        return mockPerformance.AsReadOnly();
    }
}
