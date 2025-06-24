using Microsoft.OpenApi.Models;
using System.Reflection;

namespace FlightTracker.Api.Configuration;

/// <summary>
/// Extension methods for configuring OpenAPI/Swagger documentation
/// </summary>
public static class OpenApiConfiguration
{
    /// <summary>
    /// Configure OpenAPI documentation and Swagger UI
    /// </summary>
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        
        services.AddSwaggerGen(options =>
        {
            // API Information
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1.0.0",
                Title = "Flight Tracker API",
                Description = "A comprehensive API for flight search, tracking, and price analysis",
                Contact = new OpenApiContact
                {
                    Name = "Flight Tracker Team",
                    Email = "support@flighttracker.com",
                    Url = new Uri("https://flighttracker.com/contact")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                },
                TermsOfService = new Uri("https://flighttracker.com/terms")
            });

            // Include XML comments for documentation
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // Configure security scheme for future authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            // Global security requirement (commented out for now)
            /*
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
            */            // Configure examples and schemas
            options.DescribeAllParametersInCamelCase();
              // Custom operation filters for better documentation
            // Temporarily disabled for debugging
            // options.OperationFilter<ApiResponseOperationFilter>();
            // options.DocumentFilter<ApiDocumentFilter>();
            // options.SchemaFilter<EnumSchemaFilter>();
        });

        return services;
    }    /// <summary>
    /// Configure the OpenAPI/Swagger middleware pipeline
    /// </summary>
    public static WebApplication UseOpenApiDocumentation(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(options =>
            {
                options.RouteTemplate = "api/docs/{documentName}/swagger.json";
            });
            
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/api/docs/v1/swagger.json", "Flight Tracker API v1");
                options.RoutePrefix = "api/docs";
                options.DocumentTitle = "Flight Tracker API Documentation";
                options.DefaultModelsExpandDepth(2);
                options.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
                options.DisplayRequestDuration();
                options.EnableDeepLinking();
                options.EnableFilter();
                options.ShowExtensions();
                options.EnableValidator();
                
                // Add debugging information in development
                options.ConfigObject.AdditionalItems.Add("persistAuthorization", "true");
                options.ConfigObject.AdditionalItems.Add("displayOperationId", "true");
                
                // Custom CSS for branding (only if file exists)
                try
                {
                    var cssPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "swagger-ui", "custom.css");
                    if (File.Exists(cssPath))
                    {
                        options.InjectStylesheet("/swagger-ui/custom.css");
                    }
                }
                catch
                {
                    // Ignore CSS loading errors in development
                }
            });
        }

        return app;
    }
}
