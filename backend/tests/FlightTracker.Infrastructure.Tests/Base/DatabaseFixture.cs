using FlightTracker.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace FlightTracker.Infrastructure.Tests.Base;

/// <summary>
/// Database fixture for integration tests using in-memory database
/// </summary>
public class DatabaseFixture : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly DbContextOptions<FlightDbContext> _dbContextOptions;

    public DatabaseFixture()
    {
        var services = new ServiceCollection();
        
        // Configure in-memory database for testing
        services.AddDbContext<FlightDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                   .EnableSensitiveDataLogging()
                   .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole())));

        _serviceProvider = services.BuildServiceProvider();
        
        _dbContextOptions = _serviceProvider.GetRequiredService<DbContextOptions<FlightDbContext>>();
        
        // Ensure database is created
        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public FlightDbContext CreateContext()
    {
        return new FlightDbContext(_dbContextOptions);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}
