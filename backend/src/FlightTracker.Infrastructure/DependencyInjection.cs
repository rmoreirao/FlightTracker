using FlightTracker.Domain.Repositories;
using FlightTracker.Domain.Services;
using FlightTracker.Infrastructure.Repositories;
using FlightTracker.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FlightTracker.Infrastructure;

/// <summary>
/// Extension methods for configuring infrastructure services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add infrastructure services to the service collection
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IHostEnvironment environment)
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

        return services;
    }
}
