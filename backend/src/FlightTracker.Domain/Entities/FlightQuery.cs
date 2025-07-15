namespace FlightTracker.Domain.Entities;

/// <summary>
/// Flight query entity representing search queries for analytics and caching
/// </summary>
public class FlightQuery
{
    public Guid Id { get; private set; }
    public string OriginCode { get; private set; } = string.Empty;
    public string DestinationCode { get; private set; } = string.Empty;
    public Airport? Origin { get; private set; }
    public Airport? Destination { get; private set; }
    public DateTime DepartureDate { get; private set; }
    public DateTime? ReturnDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int SearchCount { get; private set; }
    public DateTime LastSearchedAt { get; private set; }
    public string? UserId { get; private set; } // Optional tracking for user-specific search history
    public List<PriceSnapshot> PriceSnapshots { get; private set; } = new();

    public FlightQuery(
        string originCode,
        string destinationCode,
        DateTime departureDate,
        DateTime? returnDate = null)
    {
        if (string.IsNullOrWhiteSpace(originCode) || originCode.Length != 3)
            throw new ArgumentException("Origin code must be exactly 3 characters", nameof(originCode));
        
        if (string.IsNullOrWhiteSpace(destinationCode) || destinationCode.Length != 3)
            throw new ArgumentException("Destination code must be exactly 3 characters", nameof(destinationCode));
        
        if (originCode.Equals(destinationCode, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Origin and destination cannot be the same");
        
        if (departureDate.Date < DateTime.UtcNow.Date)
            throw new ArgumentException("Departure date cannot be in the past", nameof(departureDate));
        
        if (returnDate.HasValue && returnDate.Value.Date < departureDate.Date)
            throw new ArgumentException("Return date cannot be before departure date", nameof(returnDate));

        Id = Guid.NewGuid();
        OriginCode = originCode.ToUpperInvariant();
        DestinationCode = destinationCode.ToUpperInvariant();
        DepartureDate = departureDate.Date;
        ReturnDate = returnDate?.Date;
        CreatedAt = DateTime.UtcNow;
        SearchCount = 1;
        LastSearchedAt = DateTime.UtcNow;
    }

    // For EF Core
    private FlightQuery() { }

    public void IncrementSearchCount()
    {
        SearchCount++;
        LastSearchedAt = DateTime.UtcNow;
    }

    public void SetUserId(string? userId)
    {
        UserId = userId;
    }

    public void AddPriceSnapshot(PriceSnapshot priceSnapshot)
    {
        if (priceSnapshot == null)
            throw new ArgumentNullException(nameof(priceSnapshot));
        
        PriceSnapshots.Add(priceSnapshot);
    }

    public bool IsRoundTrip => ReturnDate.HasValue;

    public override string ToString()
    {
        var trip = IsRoundTrip ? $"{OriginCode}-{DestinationCode}-{OriginCode}" : $"{OriginCode}-{DestinationCode}";
        return $"{trip} {DepartureDate:yyyy-MM-dd}" + (ReturnDate.HasValue ? $" - {ReturnDate:yyyy-MM-dd}" : "");
    }
}
