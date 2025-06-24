using MediatR;
using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Services;

namespace FlightTracker.Api.Application.Queries;

/// <summary>
/// Handler for GetFlightDetailsQuery
/// </summary>
public class GetFlightDetailsQueryHandler : IRequestHandler<GetFlightDetailsQuery, Flight?>
{
    private readonly IFlightService _flightService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetFlightDetailsQueryHandler> _logger;

    public GetFlightDetailsQueryHandler(
        IFlightService flightService,
        ICacheService cacheService,
        ILogger<GetFlightDetailsQueryHandler> logger)
    {
        _flightService = flightService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Flight?> Handle(
        GetFlightDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"flight-details:{request.AirlineCode}{request.FlightNumber}:{request.DepartureDate:yyyyMMdd}";
        
        // Try get from cache first
        var cachedFlight = await _cacheService.GetAsync<Flight>(cacheKey, cancellationToken);
        if (cachedFlight != null)
        {
            _logger.LogInformation("Cache hit for flight details {FlightNumber} on {DepartureDate}", 
                request.FlightNumber, request.DepartureDate.ToString("yyyy-MM-dd"));
            return cachedFlight;
        }

        _logger.LogInformation("Cache miss for flight details {FlightNumber} on {DepartureDate}", 
            request.FlightNumber, request.DepartureDate.ToString("yyyy-MM-dd"));
        
        try
        {
            var flight = await _flightService.GetFlightDetailsAsync(
                request.FlightNumber,
                request.AirlineCode,
                request.DepartureDate,
                cancellationToken);

            if (flight != null)
            {
                // Cache result for 30 minutes
                await _cacheService.SetAsync(cacheKey, flight, TimeSpan.FromMinutes(30), cancellationToken);
            }
            
            return flight;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flight details for {FlightNumber} on {DepartureDate}",
                request.FlightNumber, request.DepartureDate.ToString("yyyy-MM-dd"));
            throw;
        }
    }
}
