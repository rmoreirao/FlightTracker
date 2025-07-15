using FlightTracker.Domain.Repositories;
using FlightTracker.Domain.Services;
using FlightTracker.Infrastructure.Repositories;
using FlightTracker.Infrastructure.Services;
using FlightTracker.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FlightTracker.Infrastructure;

/// <summary>
/// Extension methods for configuring infrastructure services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add infrastructure services to the service collection
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IHostEnvironment environment, IConfiguration configuration)
    {
        // Add Entity Framework DbContext
        services.AddDbContext<FlightDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(FlightDbContext).Assembly.GetName().Name);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });
            
            if (environment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        return AddInfrastructureServices(services, environment);
    }

    /// <summary>
    /// Add infrastructure services without DbContext (for use with Aspire)
    /// </summary>
    public static IServiceCollection AddInfrastructureWithoutDbContext(this IServiceCollection services, IHostEnvironment environment)
    {
        return AddInfrastructureServices(services, environment);
    }

    /// <summary>
    /// Add core infrastructure services (shared between both methods)
    /// </summary>
    private static IServiceCollection AddInfrastructureServices(IServiceCollection services, IHostEnvironment environment)
    {
        // Add memory cache
        services.AddMemoryCache();

        // Register repositories
        if (environment.IsDevelopment())
        {
            // Option to use either mock or real repositories in development
            var useRealRepositories = Environment.GetEnvironmentVariable("USE_REAL_REPOSITORIES") == "true";
            
            if (useRealRepositories)
            {
                services.AddScoped<IAirportRepository, EfAirportRepository>();
                services.AddScoped<IAirlineRepository, EfAirlineRepository>();
                services.AddScoped<IFlightRepository, EfFlightRepository>();
                services.AddScoped<IPriceSnapshotRepository, EfPriceSnapshotRepository>();
                services.AddScoped<IFlightQueryRepository, EfFlightQueryRepository>();
            }
            else
            {
                services.AddSingleton<IAirportRepository, MockAirportRepository>();
                services.AddSingleton<IAirlineRepository, MockAirlineRepository>();
                services.AddSingleton<IFlightRepository, MockFlightRepository>();
                services.AddSingleton<IPriceSnapshotRepository, MockPriceSnapshotRepository>();
                services.AddSingleton<IFlightQueryRepository, MockFlightQueryRepository>();
            }
        }
        else
        {
            // Production always uses real repositories
            services.AddScoped<IAirportRepository, EfAirportRepository>();
            services.AddScoped<IAirlineRepository, EfAirlineRepository>();
            services.AddScoped<IFlightRepository, EfFlightRepository>();
            services.AddScoped<IPriceSnapshotRepository, EfPriceSnapshotRepository>();
            services.AddScoped<IFlightQueryRepository, EfFlightQueryRepository>();
        }

        // Register services
        services.AddScoped<IFlightService, FlightService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IPriceAnalysisService, PriceAnalysisService>();

        // Register database initialization services for development
        if (environment.IsDevelopment())
        {
            services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();
            services.AddScoped<IDevelopmentDataSeeder, DevelopmentDataSeeder>();
        }

        return services;
    }
}
