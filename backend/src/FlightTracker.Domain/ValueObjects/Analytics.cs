namespace FlightTracker.Domain.ValueObjects;

/// <summary>
/// Value object representing price trend data for analytics (time-series data)
/// </summary>
public record PriceTrendData
{
    public DateTime Date { get; init; }
    public Money MinPrice { get; init; }
    public Money MaxPrice { get; init; }
    public Money AvgPrice { get; init; }
    public Money MedianPrice { get; init; }
    public int SampleCount { get; init; }

    public PriceTrendData(
        DateTime date,
        Money minPrice,
        Money maxPrice,
        Money avgPrice,
        Money medianPrice,
        int sampleCount)
    {
        Date = date;
        MinPrice = minPrice ?? throw new ArgumentNullException(nameof(minPrice));
        MaxPrice = maxPrice ?? throw new ArgumentNullException(nameof(maxPrice));
        AvgPrice = avgPrice ?? throw new ArgumentNullException(nameof(avgPrice));
        MedianPrice = medianPrice ?? throw new ArgumentNullException(nameof(medianPrice));
        SampleCount = sampleCount;
    }
}

/// <summary>
/// Value object representing airline performance analytics
/// </summary>
public record AirlinePerformance
{
    public string AirlineCode { get; init; }
    public string AirlineName { get; init; }
    public Money AveragePrice { get; init; }
    public Money MinPrice { get; init; }
    public Money MaxPrice { get; init; }
    public int FlightCount { get; init; }
    public double AverageStops { get; init; }

    public AirlinePerformance(
        string airlineCode,
        string airlineName,
        Money averagePrice,
        Money minPrice,
        Money maxPrice,
        int flightCount,
        double averageStops)
    {
        AirlineCode = airlineCode ?? throw new ArgumentNullException(nameof(airlineCode));
        AirlineName = airlineName ?? throw new ArgumentNullException(nameof(airlineName));
        AveragePrice = averagePrice ?? throw new ArgumentNullException(nameof(averagePrice));
        MinPrice = minPrice ?? throw new ArgumentNullException(nameof(minPrice));
        MaxPrice = maxPrice ?? throw new ArgumentNullException(nameof(maxPrice));
        FlightCount = flightCount;
        AverageStops = averageStops;
    }
}
