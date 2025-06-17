namespace FlightTracker.Domain.Entities;

/// <summary>
/// Airport entity representing airports and their basic information
/// </summary>
public class Airport
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string City { get; private set; }
    public string Country { get; private set; }
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public string? Timezone { get; private set; }

    public Airport(string code, string name, string city, string country, 
        decimal? latitude = null, decimal? longitude = null, string? timezone = null)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 3)
            throw new ArgumentException("Airport code must be exactly 3 characters", nameof(code));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Airport name is required", nameof(name));
        
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("Airport city is required", nameof(city));
        
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Airport country is required", nameof(country));

        Code = code.ToUpperInvariant();
        Name = name;
        City = city;
        Country = country;
        Latitude = latitude;
        Longitude = longitude;
        Timezone = timezone;
    }

    // For EF Core
    private Airport() { }

    public void UpdateLocation(decimal latitude, decimal longitude, string? timezone = null)
    {
        Latitude = latitude;
        Longitude = longitude;
        
        if (!string.IsNullOrWhiteSpace(timezone))
            Timezone = timezone;
    }

    public override string ToString()
    {
        return $"{Code} - {Name} ({City}, {Country})";
    }
}
