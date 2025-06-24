using FlightTracker.Domain.Entities;

namespace FlightTracker.Domain.Repositories;

/// <summary>
/// Repository interface for airport data access
/// </summary>
public interface IAirportRepository
{
    /// <summary>
    /// Get airport by IATA code
    /// </summary>
    Task<Airport?> GetByCodeAsync(string iataCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search airports by name or code
    /// </summary>
    Task<IReadOnlyList<Airport>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all airports
    /// </summary>
    Task<IReadOnlyList<Airport>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new airport
    /// </summary>
    Task<Airport> AddAsync(Airport airport, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing airport
    /// </summary>
    Task<Airport> UpdateAsync(Airport airport, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete an airport
    /// </summary>
    Task DeleteAsync(string iataCode, CancellationToken cancellationToken = default);
}
