using Microsoft.AspNetCore.Mvc;

namespace FlightTracker.Api.Controllers;

/// <summary>
/// Health check and system status operations
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Tags("Health")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Check the health status of the API
    /// </summary>
    /// <returns>Health status information</returns>
    /// <response code="200">API is healthy and operational</response>
    [HttpGet]
    [ProducesResponseType(typeof(HealthStatus), StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        return Ok(new HealthStatus
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        });
    }

    /// <summary>
    /// Get detailed system information
    /// </summary>
    /// <returns>Detailed system status</returns>
    /// <response code="200">System information retrieved successfully</response>
    [HttpGet("info")]
    [ProducesResponseType(typeof(SystemInfo), StatusCodes.Status200OK)]
    public IActionResult GetSystemInfo()
    {
        return Ok(new SystemInfo
        {
            ApplicationName = "Flight Tracker API",
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            MachineName = Environment.MachineName,
            ProcessorCount = Environment.ProcessorCount,
            WorkingSet = GC.GetTotalMemory(false),
            Timestamp = DateTime.UtcNow
        });
    }
}

/// <summary>
/// Health status response model
/// </summary>
public class HealthStatus
{
    /// <summary>
    /// Current health status
    /// </summary>
    /// <example>Healthy</example>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when health check was performed
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// API version
    /// </summary>
    /// <example>1.0.0</example>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Current environment
    /// </summary>
    /// <example>Development</example>
    public string Environment { get; set; } = string.Empty;
}

/// <summary>
/// Detailed system information response model
/// </summary>
public class SystemInfo
{
    /// <summary>
    /// Application name
    /// </summary>
    /// <example>Flight Tracker API</example>
    public string ApplicationName { get; set; } = string.Empty;

    /// <summary>
    /// Application version
    /// </summary>
    /// <example>1.0.0</example>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Current environment
    /// </summary>
    /// <example>Development</example>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// Machine name where the API is running
    /// </summary>
    /// <example>API-SERVER-01</example>
    public string MachineName { get; set; } = string.Empty;

    /// <summary>
    /// Number of processors available
    /// </summary>
    /// <example>8</example>
    public int ProcessorCount { get; set; }

    /// <summary>
    /// Current memory usage in bytes
    /// </summary>
    /// <example>134217728</example>
    public long WorkingSet { get; set; }

    /// <summary>
    /// Timestamp when information was collected
    /// </summary>
    /// <example>2024-01-15T10:30:00Z</example>
    public DateTime Timestamp { get; set; }
}
