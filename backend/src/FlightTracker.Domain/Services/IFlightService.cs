using FlightTracker.Domain.Entities;

namespace FlightTracker.Domain.Services;

/// <summary>
/// Interface for flight search and aggregation services
/// </summary>
public interface IFlightService
{
    /// <summary>
    /// Search for flights across multiple providers
    /// </summary>
    Task<IReadOnlyList<Flight>> SearchFlightsAsync(
        string originCode,
        string destinationCode,
        DateTime departureDate,
        DateTime? returnDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get flight details by flight number and date
    /// </summary>
    Task<Flight?> GetFlightDetailsAsync(
        string flightNumber,
        string airlineCode,
        DateTime departureDate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for individual flight data providers
/// </summary>
public interface IFlightProvider
{
    /// <summary>
    /// Provider name for identification
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Search flights from this specific provider
    /// </summary>
    Task<IReadOnlyList<Flight>> SearchFlightsAsync(
        string originCode,
        string destinationCode,
        DateTime departureDate,
        DateTime? returnDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if provider is available for given route
    /// </summary>
    Task<bool> IsAvailableForRouteAsync(
        string originCode,
        string destinationCode,
        CancellationToken cancellationToken = default);
}
