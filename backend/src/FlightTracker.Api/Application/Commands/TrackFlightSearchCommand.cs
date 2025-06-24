using FluentValidation;
using MediatR;

namespace FlightTracker.Api.Application.Commands;

/// <summary>
/// Command for tracking a flight search query
/// </summary>
public record TrackFlightSearchCommand(
    string OriginCode,
    string DestinationCode,
    DateTime DepartureDate,
    DateTime? ReturnDate) : IRequest<Guid>;

/// <summary>
/// Validator for TrackFlightSearchCommand
/// </summary>
public class TrackFlightSearchCommandValidator : AbstractValidator<TrackFlightSearchCommand>
{
    public TrackFlightSearchCommandValidator()
    {
        RuleFor(x => x.OriginCode)
            .NotEmpty()
            .WithMessage("Origin airport code is required")
            .Length(3)
            .WithMessage("Origin airport code must be exactly 3 characters")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Origin airport code must contain only uppercase letters");

        RuleFor(x => x.DestinationCode)
            .NotEmpty()
            .WithMessage("Destination airport code is required")
            .Length(3)
            .WithMessage("Destination airport code must be exactly 3 characters")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Destination airport code must contain only uppercase letters");

        RuleFor(x => x.DepartureDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Departure date cannot be in the past");

        RuleFor(x => x.ReturnDate)
            .GreaterThanOrEqualTo(x => x.DepartureDate)
            .WithMessage("Return date cannot be before departure date")
            .When(x => x.ReturnDate.HasValue);

        // Ensure origin and destination are different
        RuleFor(x => x)
            .Must(x => !x.OriginCode.Equals(x.DestinationCode, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Origin and destination airports must be different")
            .When(x => !string.IsNullOrWhiteSpace(x.OriginCode) && !string.IsNullOrWhiteSpace(x.DestinationCode));
    }
}
