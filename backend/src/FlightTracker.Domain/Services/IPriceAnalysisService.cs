using FlightTracker.Domain.Entities;
using FlightTracker.Domain.ValueObjects;

namespace FlightTracker.Domain.Services;

/// <summary>
/// Domain service for price analysis and trends
/// </summary>
public interface IPriceAnalysisService
{
    /// <summary>
    /// Get price trend for a specific route over time
    /// </summary>
    Task<PriceTrend> GetPriceTrendAsync(
        RouteKey route,
        DateRange dateRange,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get lowest price found for a route in the last N days
    /// </summary>
    Task<Money?> GetLowestPriceAsync(
        RouteKey route,
        int lastDays = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get price predictions for a route
    /// </summary>
    Task<PricePrediction> GetPricePredictionAsync(
        RouteKey route,
        DateTime targetDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compare current prices with historical averages
    /// </summary>
    Task<PriceComparison> ComparePricesAsync(
        IEnumerable<Flight> currentFlights,
        RouteKey route,
        int historicalDays = 30,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Value object representing price trend information
/// </summary>
public class PriceTrend
{
    public RouteKey Route { get; }
    public DateRange Period { get; }
    public Money AveragePrice { get; }
    public Money LowestPrice { get; }
    public Money HighestPrice { get; }
    public decimal TrendPercentage { get; }
    public bool IsIncreasing => TrendPercentage > 0;
    public bool IsDecreasing => TrendPercentage < 0;
    public bool IsStable => Math.Abs(TrendPercentage) < 5; // Less than 5% change

    public PriceTrend(
        RouteKey route,
        DateRange period,
        Money averagePrice,
        Money lowestPrice,
        Money highestPrice,
        decimal trendPercentage)
    {
        Route = route ?? throw new ArgumentNullException(nameof(route));
        Period = period ?? throw new ArgumentNullException(nameof(period));
        AveragePrice = averagePrice ?? throw new ArgumentNullException(nameof(averagePrice));
        LowestPrice = lowestPrice ?? throw new ArgumentNullException(nameof(lowestPrice));
        HighestPrice = highestPrice ?? throw new ArgumentNullException(nameof(highestPrice));
        TrendPercentage = trendPercentage;
    }
}

/// <summary>
/// Value object representing price prediction
/// </summary>
public class PricePrediction
{
    public RouteKey Route { get; }
    public DateTime TargetDate { get; }
    public Money PredictedPrice { get; }
    public decimal ConfidenceLevel { get; }
    public string Recommendation { get; }

    public PricePrediction(
        RouteKey route,
        DateTime targetDate,
        Money predictedPrice,
        decimal confidenceLevel,
        string recommendation)
    {
        Route = route ?? throw new ArgumentNullException(nameof(route));
        TargetDate = targetDate;
        PredictedPrice = predictedPrice ?? throw new ArgumentNullException(nameof(predictedPrice));
        ConfidenceLevel = Math.Clamp(confidenceLevel, 0, 100);
        Recommendation = recommendation ?? throw new ArgumentNullException(nameof(recommendation));
    }
}

/// <summary>
/// Value object representing price comparison results
/// </summary>
public class PriceComparison
{
    public RouteKey Route { get; }
    public Money CurrentAveragePrice { get; }
    public Money HistoricalAveragePrice { get; }
    public decimal PercentageDifference { get; }
    public bool IsGoodDeal => PercentageDifference < -10; // More than 10% below average
    public bool IsExpensive => PercentageDifference > 10; // More than 10% above average

    public PriceComparison(
        RouteKey route,
        Money currentAveragePrice,
        Money historicalAveragePrice)
    {
        Route = route ?? throw new ArgumentNullException(nameof(route));
        CurrentAveragePrice = currentAveragePrice ?? throw new ArgumentNullException(nameof(currentAveragePrice));
        HistoricalAveragePrice = historicalAveragePrice ?? throw new ArgumentNullException(nameof(historicalAveragePrice));
        
        if (currentAveragePrice.Currency != historicalAveragePrice.Currency)
            throw new ArgumentException("Currency mismatch in price comparison");

        PercentageDifference = historicalAveragePrice.Amount != 0
            ? ((currentAveragePrice.Amount - historicalAveragePrice.Amount) / historicalAveragePrice.Amount) * 100
            : 0;
    }
}
