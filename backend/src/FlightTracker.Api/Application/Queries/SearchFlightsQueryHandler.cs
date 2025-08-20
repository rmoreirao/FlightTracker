using System.Diagnostics;
using MediatR;
using FlightTracker.Api.Application.DTOs;
using FlightTracker.Domain.Services;

namespace FlightTracker.Api.Application.Queries;

/// <summary>
/// Handler for SearchFlightsQuery
/// </summary>
public class SearchFlightsQueryHandler : IRequestHandler<SearchFlightsQuery, SearchFlightsResult>
{
    private readonly IFlightService _flightService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<SearchFlightsQueryHandler> _logger;
    private static readonly ActivitySource ActivitySource = new("FlightTracker.API");

    public SearchFlightsQueryHandler(
        IFlightService flightService,
        ICacheService cacheService,
        ILogger<SearchFlightsQueryHandler> logger)
    {
        _flightService = flightService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<SearchFlightsResult> Handle(
        SearchFlightsQuery request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var cacheKey = GenerateCacheKey(request);
        
        // Try get from cache first
        //var cachedResult = await _cacheService.GetAsync<SearchFlightsResult>(cacheKey, cancellationToken);
        //if (cachedResult != null)
        //{
        //    _logger.LogInformation("Cache hit for flight search {CacheKey}", cacheKey);
        //    return cachedResult;
        //}

        _logger.LogInformation("Cache miss for flight search {CacheKey}", cacheKey);
        
        using var activity = ActivitySource.StartActivity("SearchFlights");
        activity?.SetTag("origin", request.OriginCode);
        activity?.SetTag("destination", request.DestinationCode);
        activity?.SetTag("departure_date", request.DepartureDate.ToString("yyyy-MM-dd"));
        activity?.SetTag("return_date", request.ReturnDate?.ToString("yyyy-MM-dd"));
        
        try
        {
            var flights = await _flightService.SearchFlightsAsync(
                request.OriginCode,
                request.DestinationCode,
                request.DepartureDate,
                request.ReturnDate,
                request.SearchOptions,
                cancellationToken);

            stopwatch.Stop();
            
            var result = new SearchFlightsResult(
                flights,
                DateTime.UtcNow,
                "USD", // TODO: Make this configurable
                stopwatch.Elapsed);

            // Cache result for 5 minutes
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);
            
            _logger.LogInformation(
                "Flight search completed for {Origin}-{Destination} on {DepartureDate}. Found {Count} flights in {Duration}ms",
                request.OriginCode, request.DestinationCode, request.DepartureDate.ToString("yyyy-MM-dd"), 
                flights.Count, stopwatch.ElapsedMilliseconds);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching flights for {Origin}-{Destination} on {DepartureDate}",
                request.OriginCode, request.DestinationCode, request.DepartureDate.ToString("yyyy-MM-dd"));
            throw;
        }
    }

    private static string GenerateCacheKey(SearchFlightsQuery request)
    {
        var returnDatePart = request.ReturnDate?.ToString("yyyyMMdd") ?? "null";
        var cabinsPart = request.Cabins != null && request.Cabins.Length > 0 
            ? string.Join("-", request.Cabins.OrderBy(c => c))
            : "default";
        
        return $"flights:{request.OriginCode}-{request.DestinationCode}:{request.DepartureDate:yyyyMMdd}:{returnDatePart}:{request.Adults}a{request.Children}c{request.Infants}i:{cabinsPart}";
    }
}
