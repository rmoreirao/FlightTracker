using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Repositories;
using FlightTracker.Infrastructure.Repositories.Base;
using FlightTracker.Infrastructure.Repositories.Queries.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Infrastructure.Repositories;

/// <summary>
/// EF Core and Dapper implementation of price snapshot repository optimized for TimescaleDB
/// </summary>
public class EfPriceSnapshotRepository : EfBaseRepository<PriceSnapshot, long>, IPriceSnapshotRepository
{
    private readonly PriceSnapshotQueries _dapperQueries;

    public EfPriceSnapshotRepository(
        FlightDbContext context,
        IConfiguration configuration,
        ILogger<EfPriceSnapshotRepository> logger)
        : base(context, logger)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException("ConnectionStrings:DefaultConnection is required");
        _dapperQueries = new PriceSnapshotQueries(connectionString, logger);
    }

    public async Task<IReadOnlyList<PriceSnapshot>> GetByRouteAsync(
        string originCode,
        string destinationCode,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Use Dapper for performance on this read-heavy operation
            return await _dapperQueries.GetByRouteAsync(originCode, destinationCode, startDate, endDate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price snapshots by route {Origin}-{Destination} from {StartDate} to {EndDate}",
                originCode, destinationCode, startDate, endDate);
            throw;
        }
    }

    public async Task<IReadOnlyList<PriceSnapshot>> GetByFlightAsync(
        Guid flightId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Include(ps => ps.FlightQuery)
                .Include(ps => ps.Airline)
                .Where(ps => ps.QueryId == flightId &&
                           ps.CollectedAt >= startDate &&
                           ps.CollectedAt <= endDate)
                .OrderByDescending(ps => ps.CollectedAt)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price snapshots by flight {FlightId} from {StartDate} to {EndDate}",
                flightId, startDate, endDate);
            throw;
        }
    }

    public override async Task<PriceSnapshot> AddAsync(PriceSnapshot priceSnapshot, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await base.AddAsync(priceSnapshot, cancellationToken);
            _logger.LogDebug("Added price snapshot for query {QueryId}, airline {AirlineCode}, price {Price}",
                priceSnapshot.QueryId, priceSnapshot.AirlineCode, priceSnapshot.Price);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding price snapshot for query {QueryId}",
                priceSnapshot.QueryId);
            throw;
        }
    }

    public override async Task<IReadOnlyList<PriceSnapshot>> AddRangeAsync(
        IEnumerable<PriceSnapshot> priceSnapshots,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var snapshotList = priceSnapshots.ToList();
            var result = await base.AddRangeAsync(snapshotList, cancellationToken);
            
            _logger.LogInformation("Added {Count} price snapshots in bulk", snapshotList.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding price snapshots in bulk");
            throw;
        }
    }

    public async Task<PriceSnapshot?> GetLatestByFlightAsync(Guid flightId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet
                .Include(ps => ps.FlightQuery)
                .Include(ps => ps.Airline)
                .Where(ps => ps.QueryId == flightId)
                .OrderByDescending(ps => ps.CollectedAt)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest price snapshot for flight {FlightId}", flightId);
            throw;
        }
    }

    public async Task DeleteOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
    {
        try
        {
            // For TimescaleDB, use raw SQL for efficient deletion of old data
            var deletedCount = await _dapperQueries.DeleteOlderThanAsync(cutoffDate, cancellationToken);
            
            _logger.LogInformation("Deleted {Count} price snapshots older than {CutoffDate}",
                deletedCount, cutoffDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting price snapshots older than {CutoffDate}", cutoffDate);
            throw;
        }
    }

    public async Task<IReadOnlyList<Domain.ValueObjects.PriceTrendData>> GetPriceTrendsAsync(
        string originCode,
        string destinationCode,
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dtoResults = await _dapperQueries.GetPriceTrendsAsync(originCode, destinationCode, days, cancellationToken);
            
            // Convert DTOs to domain value objects
            return dtoResults.Select(dto => new Domain.ValueObjects.PriceTrendData(
                dto.Date,
                new Domain.ValueObjects.Money(dto.MinPrice, dto.Currency),
                new Domain.ValueObjects.Money(dto.MaxPrice, dto.Currency),
                new Domain.ValueObjects.Money(dto.AvgPrice, dto.Currency),
                new Domain.ValueObjects.Money(dto.MedianPrice, dto.Currency),
                dto.SampleCount
            )).ToList().AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price trends for route {Origin}-{Destination}",
                originCode, destinationCode);
            throw;
        }
    }

    public async Task<IReadOnlyList<Domain.ValueObjects.AirlinePerformance>> GetAirlinePerformanceAsync(
        string originCode,
        string destinationCode,
        int days = 30,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var dtoResults = await _dapperQueries.GetAirlinePerformanceAsync(originCode, destinationCode, days, cancellationToken);
            
            // Convert DTOs to domain value objects
            return dtoResults.Select(dto => new Domain.ValueObjects.AirlinePerformance(
                dto.AirlineCode,
                dto.AirlineName,
                new Domain.ValueObjects.Money(dto.AveragePrice, dto.Currency),
                new Domain.ValueObjects.Money(dto.MinPrice, dto.Currency),
                new Domain.ValueObjects.Money(dto.MaxPrice, dto.Currency),
                dto.FlightCount,
                dto.AverageStops
            )).ToList().AsReadOnly();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting airline performance for route {Origin}-{Destination}",
                originCode, destinationCode);
            throw;
        }
    }

    /// <summary>
    /// Nested class for Dapper queries specific to PriceSnapshot
    /// </summary>
    private class PriceSnapshotQueries : DapperQueryRepository
    {
        public PriceSnapshotQueries(string connectionString, ILogger logger)
            : base(connectionString, logger)
        {
        }

        public async Task<IReadOnlyList<PriceSnapshot>> GetByRouteAsync(
            string originCode,
            string destinationCode,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    ps.id,
                    ps.query_id as QueryId,
                    ps.airline_code as AirlineCode,
                    ps.cabin as Cabin,
                    ps.price_amount as PriceAmount,
                    ps.price_currency as PriceCurrency,
                    ps.deep_link as DeepLink,
                    ps.flight_number as FlightNumber,
                    ps.departure_time as DepartureTime,
                    ps.arrival_time as ArrivalTime,
                    ps.stops,
                    ps.collected_at as CollectedAt
                FROM price_snapshots ps
                INNER JOIN flight_queries fq ON ps.query_id = fq.id
                WHERE fq.origin_code = @OriginCode 
                  AND fq.destination_code = @DestinationCode
                  AND ps.collected_at >= @StartDate 
                  AND ps.collected_at <= @EndDate
                ORDER BY ps.collected_at DESC
                LIMIT 1000"; // Reasonable limit for performance

            var results = await QueryAsync<PriceSnapshotDto>(sql, new
            {
                OriginCode = originCode,
                DestinationCode = destinationCode,
                StartDate = startDate,
                EndDate = endDate
            }, cancellationToken);

            // Convert DTOs to domain entities (simplified mapping)
            return results.Select(dto => PriceSnapshot.Restore(
                dto.Id,
                dto.QueryId,
                dto.AirlineCode,
                Enum.Parse<Domain.Enums.CabinClass>(dto.Cabin, true),
                new Domain.ValueObjects.Money(dto.PriceAmount, dto.PriceCurrency),
                dto.DeepLink,
                dto.FlightNumber,
                dto.DepartureTime,
                dto.ArrivalTime,
                dto.Stops,
                dto.CollectedAt
            )).ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<PriceTrendDto>> GetPriceTrendsAsync(
            string originCode,
            string destinationCode,
            int days,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    date_trunc('day', ps.collected_at) as Date,
                    MIN(ps.price_amount) as MinPrice,
                    MAX(ps.price_amount) as MaxPrice,
                    AVG(ps.price_amount) as AvgPrice,
                    percentile_cont(0.5) WITHIN GROUP (ORDER BY ps.price_amount) as MedianPrice,
                    COUNT(*) as SampleCount,
                    ps.price_currency as Currency
                FROM price_snapshots ps
                INNER JOIN flight_queries fq ON ps.query_id = fq.id
                WHERE fq.origin_code = @OriginCode 
                  AND fq.destination_code = @DestinationCode
                  AND ps.collected_at >= NOW() - INTERVAL '@Days days'
                GROUP BY date_trunc('day', ps.collected_at), ps.price_currency
                ORDER BY Date DESC";

            var results = await QueryAsync<PriceTrendDto>(sql, new
            {
                OriginCode = originCode,
                DestinationCode = destinationCode,
                Days = days
            }, cancellationToken);

            return results.ToList().AsReadOnly();
        }

        public async Task<IReadOnlyList<AirlinePerformanceDto>> GetAirlinePerformanceAsync(
            string originCode,
            string destinationCode,
            int days,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT 
                    ps.airline_code as AirlineCode,
                    a.name as AirlineName,
                    AVG(ps.price_amount) as AveragePrice,
                    MIN(ps.price_amount) as MinPrice,
                    MAX(ps.price_amount) as MaxPrice,
                    COUNT(*) as FlightCount,
                    AVG(ps.stops::numeric) as AverageStops,
                    ps.price_currency as Currency
                FROM price_snapshots ps
                INNER JOIN flight_queries fq ON ps.query_id = fq.id
                INNER JOIN airlines a ON ps.airline_code = a.code
                WHERE fq.origin_code = @OriginCode 
                  AND fq.destination_code = @DestinationCode
                  AND ps.collected_at >= NOW() - INTERVAL '@Days days'
                GROUP BY ps.airline_code, a.name, ps.price_currency
                ORDER BY AveragePrice ASC";

            var results = await QueryAsync<AirlinePerformanceDto>(sql, new
            {
                OriginCode = originCode,
                DestinationCode = destinationCode,
                Days = days
            }, cancellationToken);

            return results.ToList().AsReadOnly();
        }

        public async Task<int> DeleteOlderThanAsync(DateTime cutoffDate, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                DELETE FROM price_snapshots 
                WHERE collected_at < @CutoffDate";

            return await ExecuteAsync(sql, new { CutoffDate = cutoffDate }, cancellationToken);
        }
    }

    // DTO for Dapper mapping
    private record PriceSnapshotDto
    {
        public long Id { get; init; }
        public Guid QueryId { get; init; }
        public string AirlineCode { get; init; } = string.Empty;
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
}
