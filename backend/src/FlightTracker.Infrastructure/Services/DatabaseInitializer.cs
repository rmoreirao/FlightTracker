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
        
        try
        {
            // Ensure database exists
            await _context.Database.EnsureCreatedAsync();
            
            // Run migrations
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Database initialization failed");
            throw;
        }
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
        _logger.LogInformation("📊 Creating TimescaleDB hypertables...");
        
        try
        {
            // Create hypertables for time-series data
            await _context.Database.ExecuteSqlRawAsync(
                "SELECT create_hypertable('price_snapshots', 'created_at', if_not_exists => true);");
                
            await _context.Database.ExecuteSqlRawAsync(
                "SELECT create_hypertable('flight_queries', 'created_at', if_not_exists => true);");
            
            _logger.LogInformation("✅ TimescaleDB hypertables created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to create TimescaleDB hypertables. This is normal if TimescaleDB extension is not available.");
        }
    }
}
