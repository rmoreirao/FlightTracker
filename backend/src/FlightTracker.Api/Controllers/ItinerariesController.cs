using FlightTracker.Api.Application.DTOs;
using FlightTracker.Api.Application.Mapping;
using FlightTracker.Api.Application.Queries;
using FlightTracker.Domain.Enums;
using FlightTracker.Domain.ValueObjects;
using FluentValidation;
using MediatR;
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
    private readonly IMediator _mediator;
    private readonly ILogger<ItinerariesController> _logger;

    public ItinerariesController(IMediator mediator, ILogger<ItinerariesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Search available itineraries (one-way or round-trip) pairing flights into journey options.
    /// Supports server-side paging, basic sorting, and optional round-trip pairing when a return date is supplied.
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// <c>GET /api/v1/itineraries/search?originCode=LAX&amp;destinationCode=JFK&amp;departureDate=2025-10-01&amp;returnDate=2025-10-05&amp;sortBy=price&amp;sortOrder=asc</c>
    ///
    /// Sorting:
    /// - sortBy: price | duration (defaults price)
    /// - sortOrder: asc | desc (defaults asc)
    ///
    /// Limits (can be tuned):
    /// - maxOutbound: maximum candidate outbound flights considered
    /// - maxReturn: maximum candidate inbound flights considered (round-trip only)
    /// - maxCombos: safety cap on total pair combinations attempted
    ///
    /// Paging: page (1-based) &amp; pageSize (1..100)
    /// </remarks>
    [HttpGet("search")]
    [ProducesResponseType(typeof(SearchItinerariesResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        try
        {
            var options = ItinerarySearchOptions.Create(
                sortBy: ParseSortBy(sortBy),
                sortOrder: ParseSortOrder(sortOrder),
                page: Math.Max(1,page),
                pageSize: Math.Max(1, Math.Min(100, pageSize)),
                maxOutbound: Math.Max(1, maxOutbound),
                maxReturn: Math.Max(1, maxReturn),
                maxCombos: Math.Max(1, maxCombos));

            var query = new SearchItinerariesQuery(
                originCode.ToUpperInvariant(),
                destinationCode.ToUpperInvariant(),
                departureDate,
                returnDate,
                options);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            var problem = new ValidationProblemDetails
            {
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred."
            };
            foreach (var error in ex.Errors)
            {
                if (problem.Errors.ContainsKey(error.PropertyName))
                {
                    problem.Errors[error.PropertyName] = problem.Errors[error.PropertyName].Concat(new[]{error.ErrorMessage}).ToArray();
                }
                else
                {
                    problem.Errors.Add(error.PropertyName, new[]{error.ErrorMessage});
                }
            }
            return BadRequest(problem);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error searching itineraries {Origin}-{Destination}", originCode, destinationCode);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails{ Title="Internal Server Error", Status=500, Detail="An unexpected error occurred. Please try again later."});
        }
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
