using FlightTracker.Domain.Entities;

namespace FlightTracker.Domain.Repositories;

/// <summary>
/// Repository interface for flight query data access
/// </summary>
public interface IFlightQueryRepository
{
    /// <summary>
    /// Get flight query by ID
    /// </summary>
    Task<FlightQuery?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new flight query
    /// </summary>
    Task<FlightQuery> AddAsync(FlightQuery flightQuery, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing flight query
    /// </summary>
    Task<FlightQuery> UpdateAsync(FlightQuery flightQuery, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get queries by user (for tracking search history)
    /// </summary>
    Task<IReadOnlyList<FlightQuery>> GetByUserAsync(
        string userId,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get popular queries (for analytics)
    /// </summary>
    Task<IReadOnlyList<FlightQuery>> GetPopularQueriesAsync(
        int count = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete old queries (cleanup)
    /// </summary>
    Task DeleteOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default);
}
