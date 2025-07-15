using FlightTracker.Domain.Entities;

namespace FlightTracker.Domain.Repositories;

/// <summary>
/// Repository interface for airline data access
/// </summary>
public interface IAirlineRepository
{
    /// <summary>
    /// Get airline by IATA code
    /// </summary>
    Task<Airline?> GetByCodeAsync(string iataCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search airlines by name or code
    /// </summary>
    Task<IReadOnlyList<Airline>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all airlines
    /// </summary>
    Task<IReadOnlyList<Airline>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get airlines that have recent flight activity
    /// </summary>
    Task<IReadOnlyList<Airline>> GetActiveAirlinesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new airline
    /// </summary>
    Task<Airline> AddAsync(Airline airline, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing airline
    /// </summary>
    Task<Airline> UpdateAsync(Airline airline, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an airline
    /// </summary>
    Task DeleteAsync(string iataCode, CancellationToken cancellationToken = default);
}
