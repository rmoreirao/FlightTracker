# Flight Tracker API - OpenAPI Documentation

This document describes the OpenAPI/Swagger setup for the Flight Tracker API.

## Overview

The Flight Tracker API includes comprehensive OpenAPI 3.0 specification and Swagger UI for interactive API documentation. This provides developers with:

- Interactive API documentation
- Request/response examples
- Parameter validation details
- Authentication information
- Rate limiting details

## Accessing the Documentation

### Development Environment

When running in development mode, the Swagger UI is available at:

```
https://localhost:7018/api/docs
```

The raw OpenAPI specification (JSON) is available at:

```
https://localhost:7018/api/docs/v1/swagger.json
```

### Production Environment

In production, the Swagger UI is disabled for security reasons. However, the OpenAPI specification can still be accessed programmatically if needed.

## API Endpoints

### Health Endpoints

- `GET /api/v1/health` - Basic health check
- `GET /api/v1/health/info` - Detailed system information

### Flight Endpoints

- `GET /api/v1/flights/search` - Search for flights between airports

### Future Endpoints

- Airport search and information
- Price analytics and trends
- User management (when authentication is implemented)

## Features

### Enhanced Documentation

- **Detailed Descriptions**: Every endpoint includes comprehensive descriptions
- **Parameter Examples**: All parameters include example values and validation rules
- **Response Models**: Complete response schemas with examples
- **Error Handling**: Documented error responses with status codes

### Custom Styling

The Swagger UI includes custom Flight Tracker branding with:

- Custom CSS styling
- Flight-themed colors and icons
- Responsive design for mobile devices
- Enhanced readability

### API Organization

Endpoints are organized into logical groups:

- **Flights**: Flight search and management
- **Health**: System health and status
- **Analytics**: Price analysis and trends (future)
- **Airports**: Airport information (future)

## Configuration

### OpenAPI Configuration

The OpenAPI configuration is located in:

```
src/FlightTracker.Api/Configuration/OpenApiConfiguration.cs
```

Key features configured:

- API metadata (title, description, version)
- Contact information and licensing
- Security schemes (Bearer token ready)
- Custom operation and schema filters
- XML documentation integration

### Custom Filters

1. **ApiResponseOperationFilter**: Adds common response codes to all endpoints
2. **ApiDocumentFilter**: Adds tags, servers, and external documentation
3. **EnumSchemaFilter**: Enhances enum documentation with descriptions

### XML Documentation

XML documentation is enabled in the project file with:

```xml
<GenerateDocumentationFile>true</GenerateDocumentationFile>
<DocumentationFile>bin\$(Configuration)\$(TargetFramework)\$(AssemblyName).xml</DocumentationFile>
```

## Development Guidelines

### Adding New Endpoints

When adding new API endpoints:

1. **Add XML Documentation**: Include comprehensive `/// <summary>` comments
2. **Use Proper Attributes**: Add `[ProducesResponseType]` for all possible responses
3. **Include Examples**: Add example values in parameter descriptions
4. **Add Tags**: Use `[Tags("CategoryName")]` to organize endpoints
5. **Document Parameters**: Provide detailed parameter descriptions

### Example Controller Setup

```csharp
/// <summary>
/// Controller for flight-related operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Tags("Flights")]
public class FlightsController : ControllerBase
{
    /// <summary>
    /// Search for flights between two airports
    /// </summary>
    /// <param name="originCode">3-letter IATA code (e.g., "JFK")</param>
    /// <response code="200">Successfully retrieved flight results</response>
    /// <response code="400">Invalid search parameters</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(SearchFlightsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchFlights([FromQuery] string originCode)
    {
        // Implementation
    }
}
```

## Security Considerations

### Production Deployment

- Swagger UI is disabled in production by default
- OpenAPI specification can be made available through configuration
- Consider API versioning for future releases
- Authentication/authorization integration ready

### Rate Limiting

The API documentation includes rate limiting information:

- Search endpoints: 429 Too Many Requests response documented
- Rate limits can be configured per endpoint
- Custom headers for rate limit information

## Testing with Swagger UI

The interactive Swagger UI allows developers to:

1. **Explore Endpoints**: Browse all available API operations
2. **Test Requests**: Execute API calls directly from the browser
3. **View Responses**: See real response data and schemas
4. **Validate Parameters**: Get immediate feedback on parameter validation
5. **Download Specifications**: Export OpenAPI spec for code generation

## Integration with Development Tools

### Code Generation

The OpenAPI specification can be used with tools like:

- **OpenAPI Generator**: Generate client SDKs in multiple languages
- **Postman**: Import API specification for testing
- **Insomnia**: API testing and development
- **Azure API Management**: API gateway integration

### CI/CD Integration

Consider adding OpenAPI validation to your CI/CD pipeline:

- Validate specification format
- Check for breaking changes
- Generate client libraries automatically
- Deploy documentation to external hosting

## Troubleshooting

### Common Issues

1. **Missing XML Documentation**: Ensure `GenerateDocumentationFile` is enabled
2. **Custom CSS Not Loading**: Check static files configuration
3. **Swagger UI Not Accessible**: Verify development environment configuration
4. **Missing Response Types**: Add `[ProducesResponseType]` attributes

### Debugging

Enable detailed logging to troubleshoot OpenAPI configuration issues:

```csharp
builder.Logging.AddFilter("Microsoft.AspNetCore.OpenApi", LogLevel.Debug);
```

## Future Enhancements

Planned improvements:

- **Authentication Integration**: JWT bearer token support
- **API Versioning**: Support for multiple API versions
- **Extended Examples**: More comprehensive request/response examples
- **Custom Themes**: Additional styling options
- **Internationalization**: Multi-language documentation support
