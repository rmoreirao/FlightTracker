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
        // Airport codes aligned with frontend airports.ts file
        var knownAirports = new[]
        {
            // Major US Airports
            "LAX", "JFK", "LGA", "EWR", "ORD", "MDW", "ATL", "MIA", "DFW", "DEN",
            "SFO", "SJC", "OAK", "SEA", "LAS", "PHX", "BOS", "DTW", "MSP", "CLT",
            "IAH", "HOU", "PHL", "DCA", "IAD", "BWI", "SAN", "TPA", "MCO", "FLL",
            "PDX", "SLC", "RDU", "AUS", "BNA", "STL", "MSY", "CLE", "CMH", "IND",
            "MKE", "PIT", "CVG", "MCI", "OMA", "DSM", "RSW", "JAX", "SAV", "CHS", "MYR",
            
            // Major International Airports
            "LHR", "LGW", "STN", "LTN", "CDG", "ORY", "FRA", "MUC", "BER", "AMS",
            "ZUR", "VIE", "FCO", "MXP", "BCN", "MAD", "LIS", "ARN", "CPH", "OSL",
            "HEL", "IST", "DOH", "DXB", "AUH", "CAI", "JNB", "CPT",
            
            // Major Asian Airports
            "NRT", "HND", "KIX", "ICN", "GMP", "PEK", "PKX", "PVG", "SHA", "CAN",
            "HKG", "TPE", "SIN", "KUL", "BKK", "DMK", "CGK", "MNL", "BOM", "DEL",
            "BLR", "HYD", "MAA", "CCU",
            
            // Canadian Airports
            "YYZ", "YUL", "YVR", "YYC", "YEG", "YOW", "YHZ", "YWG",
            
            // Australian/Oceania Airports
            "SYD", "MEL", "BNE", "PER", "ADL", "DRW", "AKL", "CHC", "WLG",
            
            // South American Airports
            "GRU", "GIG", "BSB", "EZE", "SCL", "LIM", "BOG", "CCS", "UIO"
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
        // Simplified country mapping for demo purposes - aligned with airports.ts
        var countryMapping = new Dictionary<string, string>
        {
            // United States
            { "LAX", "US" }, { "JFK", "US" }, { "LGA", "US" }, { "EWR", "US" }, { "ORD", "US" },
            { "MDW", "US" }, { "ATL", "US" }, { "MIA", "US" }, { "DFW", "US" }, { "DEN", "US" },
            { "SFO", "US" }, { "SJC", "US" }, { "OAK", "US" }, { "SEA", "US" }, { "LAS", "US" },
            { "PHX", "US" }, { "BOS", "US" }, { "DTW", "US" }, { "MSP", "US" }, { "CLT", "US" },
            { "IAH", "US" }, { "HOU", "US" }, { "PHL", "US" }, { "DCA", "US" }, { "IAD", "US" },
            { "BWI", "US" }, { "SAN", "US" }, { "TPA", "US" }, { "MCO", "US" }, { "FLL", "US" },
            { "PDX", "US" }, { "SLC", "US" }, { "RDU", "US" }, { "AUS", "US" }, { "BNA", "US" },
            { "STL", "US" }, { "MSY", "US" }, { "CLE", "US" }, { "CMH", "US" }, { "IND", "US" },
            { "MKE", "US" }, { "PIT", "US" }, { "CVG", "US" }, { "MCI", "US" }, { "OMA", "US" },
            { "DSM", "US" }, { "RSW", "US" }, { "JAX", "US" }, { "SAV", "US" }, { "CHS", "US" }, { "MYR", "US" },
            
            // United Kingdom
            { "LHR", "GB" }, { "LGW", "GB" }, { "STN", "GB" }, { "LTN", "GB" },
            
            // France
            { "CDG", "FR" }, { "ORY", "FR" },
            
            // Germany
            { "FRA", "DE" }, { "MUC", "DE" }, { "BER", "DE" },
            
            // Netherlands
            { "AMS", "NL" },
            
            // Other European
            { "ZUR", "CH" }, { "VIE", "AT" }, { "FCO", "IT" }, { "MXP", "IT" }, { "BCN", "ES" },
            { "MAD", "ES" }, { "LIS", "PT" }, { "ARN", "SE" }, { "CPH", "DK" }, { "OSL", "NO" },
            { "HEL", "FI" }, { "IST", "TR" },
            
            // Middle East & Africa
            { "DOH", "QA" }, { "DXB", "AE" }, { "AUH", "AE" }, { "CAI", "EG" }, { "JNB", "ZA" }, { "CPT", "ZA" },
            
            // Asia
            { "NRT", "JP" }, { "HND", "JP" }, { "KIX", "JP" }, { "ICN", "KR" }, { "GMP", "KR" },
            { "PEK", "CN" }, { "PKX", "CN" }, { "PVG", "CN" }, { "SHA", "CN" }, { "CAN", "CN" },
            { "HKG", "HK" }, { "TPE", "TW" }, { "SIN", "SG" }, { "KUL", "MY" }, { "BKK", "TH" },
            { "DMK", "TH" }, { "CGK", "ID" }, { "MNL", "PH" }, { "BOM", "IN" }, { "DEL", "IN" },
            { "BLR", "IN" }, { "HYD", "IN" }, { "MAA", "IN" }, { "CCU", "IN" },
            
            // Canada
            { "YYZ", "CA" }, { "YUL", "CA" }, { "YVR", "CA" }, { "YYC", "CA" }, { "YEG", "CA" },
            { "YOW", "CA" }, { "YHZ", "CA" }, { "YWG", "CA" },
            
            // Australia & Oceania
            { "SYD", "AU" }, { "MEL", "AU" }, { "BNE", "AU" }, { "PER", "AU" }, { "ADL", "AU" },
            { "DRW", "AU" }, { "AKL", "NZ" }, { "CHC", "NZ" }, { "WLG", "NZ" },
            
            // South America
            { "GRU", "BR" }, { "GIG", "BR" }, { "BSB", "BR" }, { "EZE", "AR" }, { "SCL", "CL" },
            { "LIM", "PE" }, { "BOG", "CO" }, { "CCS", "VE" }, { "UIO", "EC" }
        };

        return countryMapping.TryGetValue(airportCode?.ToUpperInvariant() ?? "", out var country) 
            ? country 
            : "UNKNOWN";
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
