namespace FlightTracker.Infrastructure.Services;

/// <summary>
/// Interface for initializing the database with migrations, hypertables, and seed data
/// </summary>
public interface IDatabaseInitializer
{
    /// <summary>
    /// Initialize the database with all required setup
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Run pending database migrations
    /// </summary>
    Task MigrateAsync();

    /// <summary>
    /// Seed the database with test data
    /// </summary>
    Task SeedAsync();

    /// <summary>
    /// Create TimescaleDB hypertables for time-series data
    /// </summary>
    Task CreateTimescaleHypertablesAsync();
}
