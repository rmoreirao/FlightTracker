using FlightTracker.Domain.Entities;
using FlightTracker.Domain.ValueObjects;

namespace FlightTracker.Domain.Repositories;

/// <summary>
/// Repository interface for price snapshot data access
/// </summary>
public interface IPriceSnapshotRepository
{
    /// <summary>
    /// Get price snapshots for a specific route and date range
    /// </summary>
    Task<IReadOnlyList<PriceSnapshot>> GetByRouteAsync(
        string originCode,
        string destinationCode,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get price snapshots for a specific flight
    /// </summary>
    Task<IReadOnlyList<PriceSnapshot>> GetByFlightAsync(
        Guid flightId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new price snapshot
    /// </summary>
    Task<PriceSnapshot> AddAsync(PriceSnapshot priceSnapshot, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add multiple price snapshots
    /// </summary>
    Task<IReadOnlyList<PriceSnapshot>> AddRangeAsync(
        IEnumerable<PriceSnapshot> priceSnapshots,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest price snapshot for a flight
    /// </summary>
    Task<PriceSnapshot?> GetLatestByFlightAsync(Guid flightId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete old price snapshots (cleanup)
    /// </summary>
    Task DeleteOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get price trends over time for a route
    /// </summary>
    Task<IReadOnlyList<PriceTrendData>> GetPriceTrendsAsync(
        string originCode,
        string destinationCode,
        int days = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get airline performance analytics for a route
    /// </summary>
    Task<IReadOnlyList<AirlinePerformance>> GetAirlinePerformanceAsync(
        string originCode,
        string destinationCode,
        int days = 30,
        CancellationToken cancellationToken = default);
}
