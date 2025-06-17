namespace FlightTracker.Domain.ValueObjects;

/// <summary>
/// Value object representing a flight route between two airports
/// </summary>
public class RouteKey : IEquatable<RouteKey>
{
    public string OriginCode { get; }
    public string DestinationCode { get; }
    public bool IsRoundTrip { get; }

    public RouteKey(string originCode, string destinationCode, bool isRoundTrip = false)
    {
        if (string.IsNullOrWhiteSpace(originCode) || originCode.Length != 3)
            throw new ArgumentException("Origin code must be exactly 3 characters", nameof(originCode));
        
        if (string.IsNullOrWhiteSpace(destinationCode) || destinationCode.Length != 3)
            throw new ArgumentException("Destination code must be exactly 3 characters", nameof(destinationCode));
        
        if (originCode.Equals(destinationCode, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Origin and destination cannot be the same");

        OriginCode = originCode.ToUpperInvariant();
        DestinationCode = destinationCode.ToUpperInvariant();
        IsRoundTrip = isRoundTrip;
    }

    public static RouteKey OneWay(string originCode, string destinationCode) =>
        new(originCode, destinationCode, false);

    public static RouteKey RoundTrip(string originCode, string destinationCode) =>
        new(originCode, destinationCode, true);

    public RouteKey Reverse() => new(DestinationCode, OriginCode, IsRoundTrip);

    public string ToShortString() => 
        IsRoundTrip ? $"{OriginCode}⇄{DestinationCode}" : $"{OriginCode}→{DestinationCode}";

    public bool Equals(RouteKey? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return OriginCode == other.OriginCode && 
               DestinationCode == other.DestinationCode && 
               IsRoundTrip == other.IsRoundTrip;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is RouteKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(OriginCode, DestinationCode, IsRoundTrip);
    }

    public static bool operator ==(RouteKey? left, RouteKey? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(RouteKey? left, RouteKey? right)
    {
        return !Equals(left, right);
    }

    public override string ToString()
    {
        return $"{OriginCode}-{DestinationCode}" + (IsRoundTrip ? " (Round Trip)" : " (One Way)");
    }
}
