using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Repositories;
using FlightTracker.Domain.Services;
using FlightTracker.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Infrastructure.Services;

/// <summary>
/// Implementation of flight service using repositories
/// </summary>
public class FlightService : IFlightService
{
    private readonly IFlightRepository _flightRepository;
    private readonly IFlightQueryRepository _flightQueryRepository;
    private readonly ILogger<FlightService> _logger;

    public FlightService(
        IFlightRepository flightRepository,
        IFlightQueryRepository flightQueryRepository,
        ILogger<FlightService> logger)
    {
        _flightRepository = flightRepository;
        _flightQueryRepository = flightQueryRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Flight>> SearchFlightsAsync(
        string originCode,
        string destinationCode,
        DateTime departureDate,
        DateTime? returnDate,
        FlightSearchOptions? searchOptions = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching flights from {Origin} to {Destination} on departure date {Date} and return date {ReturnDate}",
            originCode, destinationCode, departureDate.ToString("yyyy-MM-dd"), returnDate?.ToString("yyyy-MM-dd"));

        try
        {
            // Search for flights (handles both one-way and round-trip)
            var flights = await _flightRepository.SearchAsync(
                originCode, destinationCode, departureDate, returnDate, searchOptions, cancellationToken);

            var outboundCount = flights.Count(f => f.Origin?.Code == originCode && f.Destination?.Code == destinationCode);
            var returnCount = returnDate.HasValue ? flights.Count(f => f.Origin?.Code == destinationCode && f.Destination?.Code == originCode) : 0;

            if (returnDate.HasValue)
            {
                _logger.LogInformation("Found {OutboundCount} outbound and {ReturnCount} return flights for round-trip search", 
                    outboundCount, returnCount);
            }
            else
            {
                _logger.LogInformation("Found {Count} outbound flights for one-way search", outboundCount);
            }

            return flights;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching flights from {Origin} to {Destination}",
                originCode, destinationCode);
            throw;
        }
    }

    public async Task<Flight?> GetFlightDetailsAsync(
        string flightNumber,
        string airlineCode,
        DateTime departureDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting flight details for {Airline}{FlightNumber} on {Date}",
            airlineCode, flightNumber, departureDate.ToString("yyyy-MM-dd"));

        try
        {
            var flight = await _flightRepository.GetByFlightNumberAsync(
                flightNumber, airlineCode, departureDate, cancellationToken);

            if (flight == null)
            {
                _logger.LogWarning("Flight {Airline}{FlightNumber} not found for {Date}",
                    airlineCode, flightNumber, departureDate.ToString("yyyy-MM-dd"));
            }

            return flight;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flight details for {Airline}{FlightNumber}",
                airlineCode, flightNumber);
            throw;
        }
    }
}
