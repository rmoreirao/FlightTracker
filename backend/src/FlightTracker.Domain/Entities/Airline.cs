namespace FlightTracker.Domain.Entities;

/// <summary>
/// Airline entity representing airline companies
/// </summary>
public class Airline
{
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string? LogoUrl { get; private set; }

    public Airline(string code, string name, string? logoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 2)
            throw new ArgumentException("Airline code must be exactly 2 characters", nameof(code));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Airline name is required", nameof(name));

        Code = code.ToUpperInvariant();
        Name = name;
        LogoUrl = logoUrl;
    }

    // For EF Core
    private Airline() { }

    public void UpdateLogo(string logoUrl)
    {
        LogoUrl = logoUrl;
    }

    public override string ToString()
    {
        return $"{Code} - {Name}";
    }
}
