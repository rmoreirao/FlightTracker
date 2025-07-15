using System.Linq;

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
        if (string.IsNullOrWhiteSpace(originCode))
            throw new ArgumentException("Origin cannot be null or empty", nameof(originCode));
        
        if (string.IsNullOrWhiteSpace(destinationCode))
            throw new ArgumentException("Destination cannot be null or empty", nameof(destinationCode));
        
        if (!IsValidAirportCode(originCode))
            throw new ArgumentException("Origin must be a valid 3-letter airport code", nameof(originCode));
        
        if (!IsValidAirportCode(destinationCode))
            throw new ArgumentException("Destination must be a valid 3-letter airport code", nameof(destinationCode));
        
        if (originCode.Equals(destinationCode, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Origin and destination cannot be the same");

        OriginCode = originCode.ToUpperInvariant();
        DestinationCode = destinationCode.ToUpperInvariant();
        IsRoundTrip = isRoundTrip;
    }
    
    private static bool IsValidAirportCode(string code)
    {
        return code.Length == 3 && code.All(char.IsLetter);
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
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(RouteKey? left, RouteKey? right)
    {
        return !(left == right);
    }

    public override string ToString()
    {
        return $"{OriginCode}-{DestinationCode}";
    }
}
