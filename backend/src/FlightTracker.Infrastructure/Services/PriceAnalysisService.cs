using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Repositories;
using FlightTracker.Domain.Services;
using FlightTracker.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Infrastructure.Services;

/// <summary>
/// Mock implementation of price analysis service for development
/// </summary>
public class PriceAnalysisService : IPriceAnalysisService
{
    private readonly IPriceSnapshotRepository _priceSnapshotRepository;
    private readonly ILogger<PriceAnalysisService> _logger;
    private readonly Random _random;

    public PriceAnalysisService(
        IPriceSnapshotRepository priceSnapshotRepository,
        ILogger<PriceAnalysisService> logger)
    {
        _priceSnapshotRepository = priceSnapshotRepository;
        _logger = logger;
        _random = new Random();
    }

    public async Task<PriceTrend> GetPriceTrendAsync(
        RouteKey route,
        DateRange dateRange,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting price trend for route {Route} from {Start} to {End}",
            route, dateRange.StartDate.ToString("yyyy-MM-dd"), dateRange.EndDate.ToString("yyyy-MM-dd"));

        await Task.Delay(200, cancellationToken);        // Return mock price trend
        var basePrice = GenerateMockPrice(route.OriginCode, route.DestinationCode);
        var lowestPrice = new Money(Math.Round(basePrice * 0.8m, 2), "USD");
        var highestPrice = new Money(Math.Round(basePrice * 1.3m, 2), "USD");
        var averagePrice = new Money(Math.Round(basePrice * 1.05m, 2), "USD");
        var trendPercentage = (decimal)(_random.NextDouble() * 20 - 10); // -10% to +10%
        
        return new PriceTrend(route, dateRange, averagePrice, lowestPrice, highestPrice, trendPercentage);
    }

    public async Task<Money?> GetLowestPriceAsync(
        RouteKey route,
        int lastDays = 30,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting lowest price for route {Route} in last {Days} days", route, lastDays);

        await Task.Delay(100, cancellationToken);

        // Return mock lowest price
        var basePrice = GenerateMockPrice(route.OriginCode, route.DestinationCode);
        var lowestPrice = Math.Round(basePrice * 0.7m, 2); // 30% discount
        return new Money(lowestPrice, "USD");
    }

    public async Task<PricePrediction> GetPricePredictionAsync(
        RouteKey route,
        DateTime targetDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting price prediction for route {Route} on {Date}",
            route, targetDate.ToString("yyyy-MM-dd"));

        await Task.Delay(150, cancellationToken);        // Return mock price prediction
        var basePrice = GenerateMockPrice(route.OriginCode, route.DestinationCode);
        var predictedPrice = new Money(Math.Round(basePrice * ((decimal)_random.NextDouble() * 0.4m + 0.8m), 2), "USD");
        var confidence = (decimal)(_random.NextDouble() * 30 + 70); // 70-100% confidence
        var recommendation = confidence > 85 ? "Buy now" : confidence > 75 ? "Wait for better deal" : "Monitor prices";

        return new PricePrediction(route, targetDate, predictedPrice, confidence, recommendation);
    }

    public async Task<PriceComparison> ComparePricesAsync(
        IEnumerable<Flight> currentFlights,
        RouteKey route,
        int historicalDays = 30,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Comparing prices for route {Route} with {Days} days of history", route, historicalDays);

        await Task.Delay(100, cancellationToken);        // Generate mock comparison
        var flightList = currentFlights.ToList();
        var currentPrices = flightList.Select(f => f.Price).ToList();
        var averageCurrentPrice = currentPrices.Any() 
            ? new Money(currentPrices.Average(p => p.Amount), "USD") 
            : new Money(GenerateMockPrice(route.OriginCode, route.DestinationCode), "USD");

        var historicalAverage = new Money(Math.Round(averageCurrentPrice.Amount * 1.1m, 2), "USD"); // 10% higher historical average

        return new PriceComparison(route, averageCurrentPrice, historicalAverage);
    }

    private List<Money> GenerateMockPriceHistory(RouteKey route, DateRange dateRange)
    {
        var prices = new List<Money>();
        var basePrice = GenerateMockPrice(route.OriginCode, route.DestinationCode);
        var days = (dateRange.EndDate - dateRange.StartDate).Days;

        for (int i = 0; i <= days; i++)
        {
            var variation = _random.NextDouble() * 0.4 + 0.8; // ±20% variation
            var dailyPrice = Math.Round(basePrice * (decimal)variation, 2);
            prices.Add(new Money(dailyPrice, "USD"));
        }

        return prices;
    }

    private decimal GenerateMockPrice(string originCode, string destinationCode)
    {
        // Generate consistent mock prices based on route
        var routeHash = $"{originCode}-{destinationCode}".GetHashCode();
        var baseSeed = Math.Abs(routeHash % 1000);
        return baseSeed + 200; // Base prices between 200-1200
    }
}
