using FluentValidation;

namespace FlightTracker.Api.Application.Validators;

/// <summary>
/// Custom validator for airport codes with business logic
/// </summary>
public static class AirportCodeValidator
{
    /// <summary>
    /// Validates that an airport code is properly formatted and exists
    /// </summary>
    public static IRuleBuilderOptions<T, string> MustBeValidAirportCode<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Airport code is required")
            .Length(3)
            .WithMessage("Airport code must be exactly 3 characters")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Airport code must contain only uppercase letters")
            .Must(BeKnownAirport)
            .WithMessage("Airport code is not recognized");
    }

    /// <summary>
    /// Validates that two airport codes are different
    /// </summary>
    public static IRuleBuilderOptions<T, T> MustHaveDifferentAirports<T>(
        this IRuleBuilder<T, T> ruleBuilder,
        Func<T, string> originSelector,
        Func<T, string> destinationSelector)
    {
        return ruleBuilder
            .Must(x => !string.Equals(originSelector(x), destinationSelector(x), StringComparison.OrdinalIgnoreCase))
            .WithMessage("Origin and destination airports must be different");
    }

    /// <summary>
    /// Validates that a route is commercially viable (simplified business rule)
    /// </summary>
    public static IRuleBuilderOptions<T, T> MustBeViableRoute<T>(
        this IRuleBuilder<T, T> ruleBuilder,
        Func<T, string> originSelector,
        Func<T, string> destinationSelector)
    {
        return ruleBuilder
            .Must(x => IsViableRoute(originSelector(x), destinationSelector(x)))
            .WithMessage("This route is not currently served by any airlines");
    }

    // Simple mock validation - in real implementation, this would check against a database
    private static bool BeKnownAirport(string airportCode)
    {
        // Common airport codes for validation demo
        var knownAirports = new[]
        {
            "LAX", "JFK", "LHR", "CDG", "FRA", "AMS", "MAD", "BCN", "FCO", "MUC",
            "ORD", "ATL", "DFW", "DEN", "PHX", "SFO", "SEA", "LAS", "CLT", "MIA",
            "YYZ", "YVR", "NRT", "ICN", "PVG", "HKG", "SIN", "BKK", "KUL", "CGK",
            "SYD", "MEL", "PER", "BNE", "AKL", "DXB", "DOH", "AUH", "CAI", "JNB"
        };
        
        return knownAirports.Contains(airportCode?.ToUpperInvariant());
    }

    // Simple mock validation for route viability
    private static bool IsViableRoute(string origin, string destination)
    {
        if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
            return false;

        // Mock business rule: routes within same country or between major hubs are viable
        var majorHubs = new[] { "LAX", "JFK", "LHR", "CDG", "FRA", "AMS", "NRT", "SIN", "DXB" };
        
        return majorHubs.Contains(origin.ToUpperInvariant()) || 
               majorHubs.Contains(destination.ToUpperInvariant()) ||
               GetCountryForAirport(origin) == GetCountryForAirport(destination);
    }

    private static string GetCountryForAirport(string airportCode)
    {
        // Simplified country mapping for demo purposes
        var countryMapping = new Dictionary<string, string>
        {
            { "LAX", "US" }, { "JFK", "US" }, { "ORD", "US" }, { "ATL", "US" }, { "DFW", "US" },
            { "LHR", "GB" }, { "LGW", "GB" }, { "MAN", "GB" },
            { "CDG", "FR" }, { "ORY", "FR" }, { "NCE", "FR" },
            { "FRA", "DE" }, { "MUC", "DE" }, { "TXL", "DE" },
            { "AMS", "NL" }, { "RTM", "NL" },
            { "NRT", "JP" }, { "HND", "JP" }, { "KIX", "JP" }
        };

        return countryMapping.GetValueOrDefault(airportCode?.ToUpperInvariant(), "UNKNOWN");
    }
}

/// <summary>
/// Custom validator for date ranges with business logic
/// </summary>
public static class DateRangeValidator
{
    /// <summary>
    /// Validates that a departure date is within acceptable booking window
    /// </summary>
    public static IRuleBuilderOptions<T, DateTime> MustBeWithinBookingWindow<T>(this IRuleBuilder<T, DateTime> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Departure date cannot be in the past")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(365))
            .WithMessage("Departure date cannot be more than 365 days in the future");
    }

    /// <summary>
    /// Validates that travel duration is reasonable for the route type
    /// </summary>
    public static IRuleBuilderOptions<T, T> MustHaveReasonableTravelDuration<T>(
        this IRuleBuilder<T, T> ruleBuilder,
        Func<T, DateTime> departureSelector,
        Func<T, DateTime?> returnSelector)
    {
        return ruleBuilder
            .Must(x => 
            {
                var returnDate = returnSelector(x);
                if (!returnDate.HasValue) return true; // One-way is always valid
                
                var duration = (returnDate.Value - departureSelector(x)).Days;
                return duration >= 1 && duration <= 90; // 1 day to 3 months
            })
            .WithMessage("Trip duration must be between 1 day and 90 days");
    }
}

/// <summary>
/// Custom validator for passenger counts with business logic
/// </summary>
public static class PassengerValidator
{
    /// <summary>
    /// Validates passenger distribution according to airline rules
    /// </summary>
    public static IRuleBuilderOptions<T, T> MustHaveValidPassengerDistribution<T>(
        this IRuleBuilder<T, T> ruleBuilder,
        Func<T, int> adultsSelector,
        Func<T, int> childrenSelector,
        Func<T, int> infantsSelector)
    {
        return ruleBuilder
            .Must(x =>
            {
                var adults = adultsSelector(x);
                var children = childrenSelector(x);
                var infants = infantsSelector(x);
                
                // Business rules:
                // 1. At least one adult required
                // 2. Infants cannot exceed adults (lap children)
                // 3. Total passengers cannot exceed 9 (most booking systems limit)
                // 4. All counts must be non-negative
                
                return adults >= 1 && 
                       children >= 0 && 
                       infants >= 0 && 
                       infants <= adults && 
                       (adults + children + infants) <= 9;
            })
            .WithMessage("Invalid passenger distribution: must have at least 1 adult, infants cannot exceed adults, and total passengers cannot exceed 9");
    }
}
