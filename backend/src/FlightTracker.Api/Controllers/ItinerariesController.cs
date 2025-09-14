using FlightTracker.Api.Application.DTOs;
using FlightTracker.Api.Application.Mapping;
using FlightTracker.Domain.Enums;
using FlightTracker.Domain.Services;
using FlightTracker.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace FlightTracker.Api.Controllers;

/// <summary>
/// Controller for itinerary (paired flight) searches.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Tags("Itineraries")]
public class ItinerariesController : ControllerBase
{
    private readonly IItinerarySearchService _searchService;
    private readonly ILogger<ItinerariesController> _logger;

    public ItinerariesController(IItinerarySearchService searchService, ILogger<ItinerariesController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    /// <summary>
    /// Search itineraries (one-way or round-trip). Returns composed journey options.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ItineraryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string originCode,
        [FromQuery] string destinationCode,
        [FromQuery] DateTime departureDate,
        [FromQuery] DateTime? returnDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int maxOutbound = 40,
        [FromQuery] int maxReturn = 40,
        [FromQuery] int maxCombos = 400,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null,
        CancellationToken cancellationToken = default)
    {
        var options = new ItinerarySearchOptions
        {
            Page = Math.Max(1, page),
            PageSize = Math.Max(1, Math.Min(100, pageSize)),
            MaxOutboundFlights = Math.Max(1, maxOutbound),
            MaxReturnFlights = Math.Max(1, maxReturn),
            MaxCombinations = Math.Max(1, maxCombos),
            SortBy = ParseSortBy(sortBy),
            SortOrder = ParseSortOrder(sortOrder)
        };
        var itineraries = await _searchService.SearchAsync(
            originCode.ToUpperInvariant(),
            destinationCode.ToUpperInvariant(),
            departureDate,
            returnDate,
            options,
            cancellationToken);
        return Ok(itineraries.Select(i => i.ToDto()));
    }

    private static FlightSortBy ParseSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy)) return FlightSortBy.Price;
        return sortBy.ToLowerInvariant() switch
        {
            "price" => FlightSortBy.Price,
            "duration" => FlightSortBy.Duration,
            _ => FlightSortBy.Price
        };
    }

    private static SortOrder ParseSortOrder(string? sortOrder)
    {
        if (string.IsNullOrWhiteSpace(sortOrder)) return SortOrder.Ascending;
        return sortOrder.ToLowerInvariant() switch
        {
            "desc" => SortOrder.Descending,
            _ => SortOrder.Ascending
        };
    }
}
