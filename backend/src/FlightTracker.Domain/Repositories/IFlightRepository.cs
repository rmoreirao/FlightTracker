using FlightTracker.Domain.Entities;

namespace FlightTracker.Domain.Repositories;

/// <summary>
/// Repository interface for flight data access
/// </summary>
public interface IFlightRepository
{
    /// <summary>
    /// Search for flights based on criteria
    /// </summary>
    Task<IReadOnlyList<Flight>> SearchAsync(
        string originCode,
        string destinationCode,
        DateTime departureDate,
        DateTime? returnDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get flight by ID
    /// </summary>
    Task<Flight?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get flight by flight number and date
    /// </summary>
    Task<Flight?> GetByFlightNumberAsync(
        string flightNumber,
        string airlineCode,
        DateTime departureDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new flight
    /// </summary>
    Task<Flight> AddAsync(Flight flight, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing flight
    /// </summary>
    Task<Flight> UpdateAsync(Flight flight, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a flight
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
