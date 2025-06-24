using FluentValidation;
using MediatR;

namespace FlightTracker.Api.Application.Commands;

/// <summary>
/// Command for creating a price alert
/// </summary>
public record CreatePriceAlertCommand(
    string OriginCode,
    string DestinationCode,
    DateTime DepartureDate,
    DateTime? ReturnDate,
    decimal MaxPrice,
    string Currency,
    string EmailAddress) : IRequest<Guid>;

/// <summary>
/// Validator for CreatePriceAlertCommand
/// </summary>
public class CreatePriceAlertCommandValidator : AbstractValidator<CreatePriceAlertCommand>
{
    public CreatePriceAlertCommandValidator()
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
            .WithMessage("Departure date cannot be in the past")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(365))
            .WithMessage("Departure date cannot be more than 1 year in the future");

        RuleFor(x => x.ReturnDate)
            .GreaterThanOrEqualTo(x => x.DepartureDate)
            .WithMessage("Return date cannot be before departure date")
            .LessThanOrEqualTo(x => x.DepartureDate.AddDays(30))
            .WithMessage("Return date cannot be more than 30 days after departure")
            .When(x => x.ReturnDate.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThan(0)
            .WithMessage("Maximum price must be greater than 0")
            .LessThanOrEqualTo(50000)
            .WithMessage("Maximum price cannot exceed 50,000");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be exactly 3 characters")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be a valid 3-letter ISO code")
            .Must(BeValidCurrency)
            .WithMessage("Currency must be one of: USD, EUR, GBP, CAD, AUD, JPY");

        RuleFor(x => x.EmailAddress)
            .NotEmpty()
            .WithMessage("Email address is required")
            .EmailAddress()
            .WithMessage("Email address must be valid")
            .MaximumLength(254)
            .WithMessage("Email address cannot exceed 254 characters");

        // Business rule: Origin and destination must be different
        RuleFor(x => x)
            .Must(x => !x.OriginCode.Equals(x.DestinationCode, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Origin and destination airports must be different")
            .OverridePropertyName("DestinationCode")
            .When(x => !string.IsNullOrWhiteSpace(x.OriginCode) && !string.IsNullOrWhiteSpace(x.DestinationCode));
    }

    private static bool BeValidCurrency(string currency)
    {
        var validCurrencies = new[] { "USD", "EUR", "GBP", "CAD", "AUD", "JPY" };
        return validCurrencies.Contains(currency?.ToUpperInvariant());
    }
}
