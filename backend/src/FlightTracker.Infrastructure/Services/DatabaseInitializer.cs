using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FlightTracker.Infrastructure.Configuration;

namespace FlightTracker.Infrastructure.Services;

/// <summary>
/// Implementation of database initializer for development environment
/// </summary>
public class DatabaseInitializer : IDatabaseInitializer
{
    private readonly FlightDbContext _context;
    private readonly IDevelopmentDataSeeder _seeder;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly DatabaseOptions _options;

    public DatabaseInitializer(
        FlightDbContext context,
        IDevelopmentDataSeeder seeder,
        ILogger<DatabaseInitializer> logger,
        IOptions<DatabaseOptions> options)
    {
        _context = context;
        _seeder = seeder;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("🚀 Initializing development database...");

        // Run migrations (this will also ensure database exists)
        if (_options.AutoMigrateOnStartup)
        {
            await MigrateAsync();
        }

        // Create TimescaleDB hypertables
        if (_options.CreateHypertables)
        {
            await CreateTimescaleHypertablesAsync();
        }

        // Seed test data
        if (_options.SeedTestDataOnStartup)
        {
            await SeedAsync();
        }

        _logger.LogInformation("✅ Database initialization completed successfully");

    }

    public async Task MigrateAsync()
    {
        _logger.LogInformation("🔄 Running database migrations...");

        var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            _logger.LogInformation("Found {Count} pending migrations: {Migrations}",
                pendingMigrations.Count(), string.Join(", ", pendingMigrations));

            await _context.Database.MigrateAsync();
            _logger.LogInformation("✅ Database migrations completed");
        }
        else
        {
            _logger.LogInformation("📝 Database is up to date, no migrations needed");
        }
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("🌱 Starting test data seeding...");
        await _seeder.SeedAsync();
        _logger.LogInformation("✅ Test data seeding completed");
    }

    public async Task CreateTimescaleHypertablesAsync()
    {
        try
        {
            _logger.LogInformation("📊 Creating TimescaleDB hypertables...");

            // Check if TimescaleDB extension is available
            var extensionCheck = await _context.Database.ExecuteSqlRawAsync(
                "SELECT COUNT(*) FROM pg_extension WHERE extname = 'timescaledb'");

            if (extensionCheck == 0)
            {
                _logger.LogInformation("🔧 TimescaleDB extension not found, creating it...");
                
                // Try to create TimescaleDB extension
                await _context.Database.ExecuteSqlRawAsync(
                    "CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;");
                
                _logger.LogInformation("✅ TimescaleDB extension created successfully");
            }
            else
            {
                _logger.LogInformation("✅ TimescaleDB extension is already available");
            }

            // Create hypertables for time-series data (using quoted table names for EF Core)
            // Note: TimescaleDB requires removing foreign key constraints before creating hypertables
            
            // Drop foreign key constraints for PriceSnapshots table
            await _context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE \"PriceSnapshots\" DROP CONSTRAINT IF EXISTS \"FK_PriceSnapshots_Airlines_AirlineCode\";");
            await _context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE \"PriceSnapshots\" DROP CONSTRAINT IF EXISTS \"FK_PriceSnapshots_FlightQueries_QueryId\";");
            
            // Drop foreign key constraints for FlightQueries table
            await _context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE \"FlightQueries\" DROP CONSTRAINT IF EXISTS \"FK_FlightQueries_Airports_DestinationCode\";");
            await _context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE \"FlightQueries\" DROP CONSTRAINT IF EXISTS \"FK_FlightQueries_Airports_OriginCode\";");
            
            // Drop primary key constraint for FlightQueries table (TimescaleDB requirement)
            await _context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE \"FlightQueries\" DROP CONSTRAINT IF EXISTS \"PK_FlightQueries\";");
            
            _logger.LogInformation("🔗 Removed foreign key and primary key constraints for TimescaleDB compatibility");

            // Create hypertables
            await _context.Database.ExecuteSqlRawAsync(
                "SELECT create_hypertable('\"PriceSnapshots\"', 'CollectedAt', if_not_exists => true);");

            await _context.Database.ExecuteSqlRawAsync(
                "SELECT create_hypertable('\"FlightQueries\"', 'CreatedAt', if_not_exists => true);");

            _logger.LogInformation("✅ TimescaleDB hypertables created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Could not create TimescaleDB hypertables. Continuing without time-series optimization...");
            // Don't throw - the application should work without TimescaleDB features
        }
    }
}
