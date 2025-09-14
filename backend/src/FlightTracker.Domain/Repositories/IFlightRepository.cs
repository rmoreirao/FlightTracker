using FlightTracker.Domain.Entities;
using FlightTracker.Domain.ValueObjects;

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
        FlightSearchOptions? searchOptions = null,
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

    /// <summary>
    /// Get flights by route within a date range
    /// </summary>
    Task<IReadOnlyList<Flight>> GetByRouteAsync(
        string originCode,
        string destinationCode,
        int days = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent flights
    /// </summary>
    Task<IReadOnlyList<Flight>> GetRecentFlightsAsync(
        int count = 50,
        CancellationToken cancellationToken = default);
}
