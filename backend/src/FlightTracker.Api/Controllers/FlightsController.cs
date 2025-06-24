using MediatR;
using Microsoft.AspNetCore.Mvc;
using FlightTracker.Api.Application.Queries;
using FlightTracker.Api.Application.DTOs;
using FluentValidation;

namespace FlightTracker.Api.Controllers;

/// <summary>
/// Controller for flight-related operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Tags("Flights")]
public class FlightsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<FlightsController> _logger;    public FlightsController(IMediator mediator, ILogger<FlightsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Search for flights between two airports
    /// </summary>
    /// <param name="originCode">3-letter IATA airport code for origin (e.g., "JFK", "LAX")</param>
    /// <param name="destinationCode">3-letter IATA airport code for destination (e.g., "JFK", "LAX")</param>
    /// <param name="departureDate">Departure date in YYYY-MM-DD format</param>
    /// <param name="returnDate">Return date in YYYY-MM-DD format (optional for round-trip flights)</param>
    /// <param name="cabins">Comma-separated list of cabin classes (Economy, PremiumEconomy, Business, First)</param>
    /// <param name="adults">Number of adult passengers (18+ years, minimum 1, maximum 9)</param>
    /// <param name="children">Number of child passengers (2-17 years, maximum 8)</param>
    /// <param name="infants">Number of infant passengers (0-2 years, maximum adults count)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Flight search results with available flights and pricing information</returns>
    /// <response code="200">Successfully retrieved flight search results</response>
    /// <response code="400">Invalid search parameters provided</response>
    /// <response code="429">Rate limit exceeded - too many requests</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(SearchFlightsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchFlights(
        [FromQuery] string originCode,
        [FromQuery] string destinationCode,
        [FromQuery] DateTime departureDate,
        [FromQuery] DateTime? returnDate = null,
        [FromQuery] string? cabins = null,
        [FromQuery] int adults = 1,
        [FromQuery] int children = 0,
        [FromQuery] int infants = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cabinArray = !string.IsNullOrWhiteSpace(cabins) 
                ? cabins.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim().ToLowerInvariant())
                    .ToArray()
                : null;

            var query = new SearchFlightsQuery(
                originCode?.ToUpperInvariant() ?? string.Empty,
                destinationCode?.ToUpperInvariant() ?? string.Empty,
                departureDate,
                returnDate,
                cabinArray,
                adults,
                children,
                infants);

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            var problemDetails = new ValidationProblemDetails
            {
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred."
            };

            foreach (var error in ex.Errors)
            {
                if (problemDetails.Errors.ContainsKey(error.PropertyName))
                {
                    problemDetails.Errors[error.PropertyName] = 
                        problemDetails.Errors[error.PropertyName].Concat(new[] { error.ErrorMessage }).ToArray();
                }
                else
                {
                    problemDetails.Errors.Add(error.PropertyName, new[] { error.ErrorMessage });
                }
            }

            return BadRequest(problemDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while searching flights");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An unexpected error occurred. Please try again later."
                });
        }
    }

    /// <summary>
    /// Get flight details by flight number
    /// </summary>
    /// <param name="flightNumber">Flight number</param>
    /// <param name="airlineCode">2-letter airline code</param>
    /// <param name="departureDate">Departure date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Flight details or null if not found</returns>
    [HttpGet("{airlineCode}/{flightNumber}")]
    [ProducesResponseType(typeof(Domain.Entities.Flight), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFlightDetails(
        string flightNumber,
        string airlineCode,
        [FromQuery] DateTime departureDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetFlightDetailsQuery(
                flightNumber?.ToUpperInvariant() ?? string.Empty,
                airlineCode?.ToUpperInvariant() ?? string.Empty,
                departureDate);

            var flight = await _mediator.Send(query, cancellationToken);
            
            if (flight == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Flight not found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = $"Flight {airlineCode}{flightNumber} on {departureDate:yyyy-MM-dd} was not found."
                });
            }

            return Ok(flight);
        }
        catch (ValidationException ex)
        {
            var problemDetails = new ValidationProblemDetails
            {
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred."
            };

            foreach (var error in ex.Errors)
            {
                if (problemDetails.Errors.ContainsKey(error.PropertyName))
                {
                    problemDetails.Errors[error.PropertyName] = 
                        problemDetails.Errors[error.PropertyName].Concat(new[] { error.ErrorMessage }).ToArray();
                }
                else
                {
                    problemDetails.Errors.Add(error.PropertyName, new[] { error.ErrorMessage });
                }
            }

            return BadRequest(problemDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while getting flight details for {FlightNumber}", 
                $"{airlineCode}{flightNumber}");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An unexpected error occurred. Please try again later."
                });
        }
    }
}
