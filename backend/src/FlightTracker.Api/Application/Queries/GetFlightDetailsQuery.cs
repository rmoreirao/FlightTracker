using FluentValidation;
using MediatR;
using FlightTracker.Domain.Entities;

namespace FlightTracker.Api.Application.Queries;

/// <summary>
/// Query for getting flight details by flight number
/// </summary>
public record GetFlightDetailsQuery(
    string FlightNumber,
    string AirlineCode,
    DateTime DepartureDate) : IRequest<Flight?>;

/// <summary>
/// Validator for GetFlightDetailsQuery
/// </summary>
public class GetFlightDetailsQueryValidator : AbstractValidator<GetFlightDetailsQuery>
{
    public GetFlightDetailsQueryValidator()
    {
        RuleFor(x => x.FlightNumber)
            .NotEmpty()
            .WithMessage("Flight number is required")
            .MaximumLength(10)
            .WithMessage("Flight number cannot exceed 10 characters")
            .Matches("^[A-Z0-9]+$")
            .WithMessage("Flight number must contain only uppercase letters and numbers");

        RuleFor(x => x.AirlineCode)
            .NotEmpty()
            .WithMessage("Airline code is required")
            .Length(2)
            .WithMessage("Airline code must be exactly 2 characters")
            .Matches("^[A-Z]{2}$")
            .WithMessage("Airline code must contain only uppercase letters");

        RuleFor(x => x.DepartureDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date.AddDays(-1))
            .WithMessage("Departure date cannot be more than 1 day in the past")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(365))
            .WithMessage("Departure date cannot be more than 1 year in the future");
    }
}
