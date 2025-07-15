namespace FlightTracker.Infrastructure.Repositories.Queries.DTOs;

/// <summary>
/// DTO for flight search results optimized for Dapper queries
/// </summary>
public record FlightSearchResultDto
{
    public Guid QueryId { get; init; }
    public string OriginCode { get; init; } = string.Empty;
    public string DestinationCode { get; init; } = string.Empty;
    public string OriginName { get; init; } = string.Empty;
    public string DestinationName { get; init; } = string.Empty;
    public DateTime DepartureDate { get; init; }
    public DateTime? ReturnDate { get; init; }
    
    // Price snapshot data
    public long SnapshotId { get; init; }
    public string AirlineCode { get; init; } = string.Empty;
    public string AirlineName { get; init; } = string.Empty;
    public string Cabin { get; init; } = string.Empty;
    public decimal PriceAmount { get; init; }
    public string PriceCurrency { get; init; } = string.Empty;
    public string? DeepLink { get; init; }
    public string? FlightNumber { get; init; }
    public DateTime? DepartureTime { get; init; }
    public DateTime? ArrivalTime { get; init; }
    public int Stops { get; init; }
    public DateTime CollectedAt { get; init; }
}

/// <summary>
/// DTO for price trend analysis
/// </summary>
public record PriceTrendDto
{
    public DateTime Date { get; init; }
    public decimal MinPrice { get; init; }
    public decimal MaxPrice { get; init; }
    public decimal AvgPrice { get; init; }
    public decimal MedianPrice { get; init; }
    public int SampleCount { get; init; }
    public string Currency { get; init; } = string.Empty;
}

/// <summary>
/// DTO for popular route analytics
/// </summary>
public record PopularRouteDto
{
    public string OriginCode { get; init; } = string.Empty;
    public string DestinationCode { get; init; } = string.Empty;
    public string OriginName { get; init; } = string.Empty;
    public string DestinationName { get; init; } = string.Empty;
    public int SearchCount { get; init; }
    public DateTime LastSearched { get; init; }
    public decimal? AveragePrice { get; init; }
    public string? Currency { get; init; }
}

/// <summary>
/// DTO for airline performance analytics
/// </summary>
public record AirlinePerformanceDto
{
    public string AirlineCode { get; init; } = string.Empty;
    public string AirlineName { get; init; } = string.Empty;
    public decimal AveragePrice { get; init; }
    public decimal MinPrice { get; init; }
    public decimal MaxPrice { get; init; }
    public int FlightCount { get; init; }
    public double AverageStops { get; init; }
    public string Currency { get; init; } = string.Empty;
}

/// <summary>
/// DTO for price snapshot aggregations
/// </summary>
public record PriceSnapshotStatsDto
{
    public Guid QueryId { get; init; }
    public string Route { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public int TotalSnapshots { get; init; }
    public decimal MinPrice { get; init; }
    public decimal MaxPrice { get; init; }
    public decimal AvgPrice { get; init; }
    public int UniqueAirlines { get; init; }
    public string Currency { get; init; } = string.Empty;
}
