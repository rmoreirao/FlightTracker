using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel;
using System.Reflection;

namespace FlightTracker.Api.Configuration;

/// <summary>
/// Operation filter to enhance API response documentation
/// </summary>
public class ApiResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Add common response codes if not already present
        if (!operation.Responses.ContainsKey("400"))
        {
            operation.Responses.Add("400", new OpenApiResponse
            {
                Description = "Bad Request - Invalid input parameters"
            });
        }

        if (!operation.Responses.ContainsKey("500"))
        {
            operation.Responses.Add("500", new OpenApiResponse
            {
                Description = "Internal Server Error - An error occurred while processing the request"
            });
        }

        // Add rate limiting response for search endpoints
        if (operation.OperationId?.Contains("Search", StringComparison.OrdinalIgnoreCase) == true)
        {
            if (!operation.Responses.ContainsKey("429"))
            {
                operation.Responses.Add("429", new OpenApiResponse
                {
                    Description = "Too Many Requests - Rate limit exceeded"
                });
            }
        }
    }
}

/// <summary>
/// Document filter to add additional API documentation
/// </summary>
public class ApiDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Add tags for better organization
        swaggerDoc.Tags = new List<OpenApiTag>
        {
            new OpenApiTag
            {
                Name = "Flights",
                Description = "Flight search and management operations"
            },
            new OpenApiTag
            {
                Name = "Airports",
                Description = "Airport information and search operations"
            },
            new OpenApiTag
            {
                Name = "Analytics",
                Description = "Price analytics and trend analysis operations"
            },
            new OpenApiTag
            {
                Name = "Health",
                Description = "Health check and system status operations"
            }
        };

        // Add servers information
        if (swaggerDoc.Servers?.Any() != true)
        {
            swaggerDoc.Servers = new List<OpenApiServer>
            {
                new OpenApiServer
                {
                    Url = "https://localhost:7018",
                    Description = "Development server"
                },
                new OpenApiServer
                {
                    Url = "https://api.flighttracker.com",
                    Description = "Production server"
                }
            };
        }

        // Add external documentation
        swaggerDoc.ExternalDocs = new OpenApiExternalDocs
        {
            Description = "Flight Tracker Documentation",
            Url = new Uri("https://docs.flighttracker.com")
        };
    }
}

/// <summary>
/// Schema filter to enhance enum documentation
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            var enumNames = Enum.GetNames(context.Type);
            var enumValues = Enum.GetValues(context.Type);
            
            for (int i = 0; i < enumNames.Length; i++)
            {
                var enumMember = context.Type.GetMember(enumNames[i]).FirstOrDefault();
                var descriptionAttribute = enumMember?.GetCustomAttribute<DescriptionAttribute>();
                
                schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(enumNames[i]));
            }

            // Add description with all possible values
            var descriptions = new List<string>();
            for (int i = 0; i < enumNames.Length; i++)
            {
                var enumMember = context.Type.GetMember(enumNames[i]).FirstOrDefault();
                var descriptionAttribute = enumMember?.GetCustomAttribute<DescriptionAttribute>();
                var description = descriptionAttribute?.Description ?? enumNames[i];
                descriptions.Add($"{enumNames[i]} = {Convert.ToInt32(enumValues.GetValue(i))} ({description})");
            }

            if (descriptions.Any())
            {
                schema.Description = $"Possible values:\n{string.Join("\n", descriptions)}";
            }
        }
    }
}
