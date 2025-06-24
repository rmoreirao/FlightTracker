using FluentValidation;
using MediatR;
using FlightTracker.Api.Application.DTOs;
using FlightTracker.Api.Application.Validators;

namespace FlightTracker.Api.Application.Queries;

/// <summary>
/// Query for searching flights
/// </summary>
public record SearchFlightsQuery(
    string OriginCode,
    string DestinationCode,
    DateTime DepartureDate,
    DateTime? ReturnDate,
    string[]? Cabins = null,
    int Adults = 1,
    int Children = 0,
    int Infants = 0) : IRequest<SearchFlightsResult>;

/// <summary>
/// Validator for SearchFlightsQuery using custom validation extensions
/// </summary>
public class SearchFlightsQueryValidator : AbstractValidator<SearchFlightsQuery>
{
    public SearchFlightsQueryValidator()
    {
        // Validate origin airport code
        RuleFor(x => x.OriginCode)
            .MustBeValidAirportCode();

        // Validate destination airport code
        RuleFor(x => x.DestinationCode)
            .MustBeValidAirportCode();

        // Validate departure date is within booking window
        RuleFor(x => x.DepartureDate)
            .MustBeWithinBookingWindow();

        // Validate return date if provided
        RuleFor(x => x.ReturnDate)
            .GreaterThanOrEqualTo(x => x.DepartureDate)
            .WithMessage("Return date cannot be before departure date")
            .When(x => x.ReturnDate.HasValue);

        // Validate cabin classes if provided
        RuleFor(x => x.Cabins)
            .Must(cabins => cabins == null || cabins.All(c => IsValidCabin(c)))
            .WithMessage("Invalid cabin class specified. Valid values are: economy, premium_economy, business, first")
            .When(x => x.Cabins != null && x.Cabins.Length > 0);

        // Validate passenger distribution using custom validator
        RuleFor(x => x)
            .MustHaveValidPassengerDistribution(x => x.Adults, x => x.Children, x => x.Infants);

        // Validate airports are different using custom validator
        RuleFor(x => x)
            .MustHaveDifferentAirports(x => x.OriginCode, x => x.DestinationCode);

        // Validate route is commercially viable
        RuleFor(x => x)
            .MustBeViableRoute(x => x.OriginCode, x => x.DestinationCode);

        // Validate travel duration is reasonable
        RuleFor(x => x)
            .MustHaveReasonableTravelDuration(x => x.DepartureDate, x => x.ReturnDate);
    }

    private static bool IsValidCabin(string cabin)
    {
        var validCabins = new[] { "economy", "premium_economy", "business", "first" };
        return validCabins.Contains(cabin.ToLowerInvariant());
    }
}
