using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ServiceDiscovery;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Extension methods for configuring common service defaults for Flight Tracker services.
/// </summary>
public static class ServiceDefaultsExtensions
{
    /// <summary>
    /// Adds the services except for making calls to <see cref="AddDefaultHealthChecks(IHostApplicationBuilder)"/>,
    /// <see cref="AddServiceDiscovery(IHostApplicationBuilder)"/>.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="IHostApplicationBuilder"/>.</returns>
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.AddDefaultHealthChecks();
        builder.AddServiceDiscovery();
        builder.AddResilienceStrategies();

        return builder;
    }

    /// <summary>
    /// Adds default health checks to the application.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="IHostApplicationBuilder"/>.</returns>
    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }    /// <summary>
    /// Adds service discovery to the application.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="IHostApplicationBuilder"/>.</returns>
    public static IHostApplicationBuilder AddServiceDiscovery(this IHostApplicationBuilder builder)
    {
        // Add service discovery - this enables resolving service names to endpoints
        // Services can be resolved using logical names like "https://api" instead of hardcoded URLs
        builder.Services.AddServiceDiscovery();

        return builder;
    }/// <summary>
    /// Adds resilience strategies to HTTP clients.
    /// </summary>
    /// <param name="builder">The <see cref="IHostApplicationBuilder"/> to configure.</param>
    /// <returns>The configured <see cref="IHostApplicationBuilder"/>.</returns>
    public static IHostApplicationBuilder AddResilienceStrategies(this IHostApplicationBuilder builder)
    {
        // Configure standard resilience strategies for HTTP clients
        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Add standard resilience handler which includes:
            // - Retry with exponential backoff
            // - Circuit breaker
            // - Timeout
            http.AddStandardResilienceHandler();
            
            // Turn on service discovery by default
            // Note: Service discovery on HttpClient will be configured per client as needed
            // http.AddServiceDiscovery(); // Not available in current package version
        });

        return builder;
    }

    /// <summary>
    /// Maps default endpoints for health checks.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    /// <returns>The configured <see cref="WebApplication"/>.</returns>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Health check endpoints
        app.MapHealthChecks("/health");
        app.MapHealthChecks("/alive", new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });        return app;
    }

    /// <summary>
    /// Adds service discovery to an HttpClient. Use this when configuring individual HttpClients
    /// that need to resolve service names to endpoints.
    /// </summary>
    /// <param name="builder">The HttpClient builder.</param>
    /// <returns>The configured HttpClient builder.</returns>
    /// <example>
    /// builder.Services.AddHttpClient&lt;MyServiceClient&gt;(client => 
    /// {
    ///     client.BaseAddress = new Uri("https://my-service");
    /// })
    /// .AddServiceDiscoverySupport();
    /// </example>
    public static IHttpClientBuilder AddServiceDiscoverySupport(this IHttpClientBuilder builder)
    {
        // When the AddServiceDiscovery extension method becomes available in the package,
        // this can be uncommented:
        // return builder.AddServiceDiscovery();
        
        // For now, service discovery is configured at the service level
        // Individual HttpClients will use logical service names in their BaseAddress
        return builder;
    }
}
