namespace FlightTracker.Infrastructure.Services;

/// <summary>
/// Interface for seeding development test data
/// </summary>
public interface IDevelopmentDataSeeder
{
    /// <summary>
    /// Seed all development test data
    /// </summary>
    Task SeedAsync();

    /// <summary>
    /// Seed airport reference data
    /// </summary>
    Task SeedAirportsAsync();

    /// <summary>
    /// Seed airline reference data
    /// </summary>
    Task SeedAirlinesAsync();

    /// <summary>
    /// Seed sample flight schedules
    /// </summary>
    Task SeedFlightsAsync();

    /// <summary>
    /// Seed sample flight search queries
    /// </summary>
    Task SeedFlightQueriesAsync();

    /// <summary>
    /// Seed historical price data
    /// </summary>
    Task SeedPriceHistoryAsync();
}
