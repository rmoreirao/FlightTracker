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
            // Use mock repositories in development
            services.AddSingleton<IAirportRepository, MockAirportRepository>();
            services.AddSingleton<IAirlineRepository, MockAirlineRepository>();
            services.AddSingleton<IFlightRepository, MockFlightRepository>();
            services.AddSingleton<IPriceSnapshotRepository, MockPriceSnapshotRepository>();
            services.AddSingleton<IFlightQueryRepository, MockFlightQueryRepository>();
        }
        else
        {
            // TODO: Add real repository implementations for production
            // services.AddScoped<IAirportRepository, EfAirportRepository>();
            // services.AddScoped<IAirlineRepository, EfAirlineRepository>();
            // services.AddScoped<IFlightRepository, EfFlightRepository>();
            // services.AddScoped<IPriceSnapshotRepository, EfPriceSnapshotRepository>();
            // services.AddScoped<IFlightQueryRepository, EfFlightQueryRepository>();
            
            // For now, use mock repositories in all environments
            services.AddSingleton<IAirportRepository, MockAirportRepository>();
            services.AddSingleton<IAirlineRepository, MockAirlineRepository>();
            services.AddSingleton<IFlightRepository, MockFlightRepository>();
            services.AddSingleton<IPriceSnapshotRepository, MockPriceSnapshotRepository>();
            services.AddSingleton<IFlightQueryRepository, MockFlightQueryRepository>();
        }

        // Register services
        services.AddScoped<IFlightService, FlightService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IPriceAnalysisService, PriceAnalysisService>();

        return services;
    }
}
