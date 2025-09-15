namespace FlightTracker.Infrastructure.Configuration;

/// <summary>
/// Configuration options for database behavior in different environments
/// </summary>
public class DatabaseOptions
{
    public const string SectionName = "DatabaseOptions";

    /// <summary>
    /// Whether to automatically run migrations on application startup
    /// </summary>
    public bool AutoMigrateOnStartup { get; set; } = false;

    /// <summary>
    /// Whether to seed test data on application startup
    /// </summary>
    public bool SeedTestDataOnStartup { get; set; } = false;

    /// <summary>
    /// Whether to force re-seeding by deleting all existing data first
    /// Only applies when SeedTestDataOnStartup is true
    /// </summary>
    public bool ForceReseedData { get; set; } = false;

    /// <summary>
    /// Whether to enable sensitive data logging for Entity Framework
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Whether to enable detailed errors for Entity Framework
    /// </summary>
    public bool EnableDetailedErrors { get; set; } = false;

    /// <summary>
    /// Command timeout in seconds for database operations
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// Whether to create TimescaleDB hypertables for time-series data
    /// </summary>
    public bool CreateHypertables { get; set; } = true;

    // ---- Itinerary generation options ----
    /// <summary>
    /// Whether to generate itineraries (derived from flights) during seeding
    /// </summary>
    public bool GenerateItineraries { get; set; } = true;

    /// <summary>
    /// Max one-way itineraries per (route, day)
    /// </summary>
    public int MaxOneWayPerRoutePerDay { get; set; } = 100;

    /// <summary>
    /// Max round-trip itineraries per (route, departure day)
    /// </summary>
    public int MaxRoundTripsPerRoutePerDay { get; set; } = 100;

    /// <summary>
    /// Minimum days after outbound for return flight
    /// </summary>
    public int MinReturnTripDays { get; set; } = 2;

    /// <summary>
    /// Maximum days after outbound for return flight
    /// </summary>
    public int MaxReturnTripDays { get; set; } = 14;
}
