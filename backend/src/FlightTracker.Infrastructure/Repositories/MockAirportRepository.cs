using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Infrastructure.Repositories;

/// <summary>
/// Mock implementation of airport repository for development
/// </summary>
public class MockAirportRepository : IAirportRepository
{
    private readonly ILogger<MockAirportRepository> _logger;
    private readonly List<Airport> _airports;

    public MockAirportRepository(ILogger<MockAirportRepository> logger)
    {
        _logger = logger;
        _airports = InitializeMockAirports();
    }    public async Task<Airport?> GetByCodeAsync(string iataCode, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken);
        return _airports.FirstOrDefault(a => a.Code == iataCode);
    }

    public async Task<IReadOnlyList<Airport>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        var lowerSearchTerm = searchTerm.ToLowerInvariant();
        
        return _airports.Where(a =>
            a.Code.Contains(lowerSearchTerm, StringComparison.OrdinalIgnoreCase) ||
            a.Name.Contains(lowerSearchTerm, StringComparison.OrdinalIgnoreCase) ||
            a.City.Contains(lowerSearchTerm, StringComparison.OrdinalIgnoreCase) ||
            a.Country.Contains(lowerSearchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    public async Task<IReadOnlyList<Airport>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        return _airports.AsReadOnly();
    }

    public async Task<Airport> AddAsync(Airport airport, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken);
        _airports.Add(airport);
        _logger.LogInformation("Added airport {Code}", airport.Code);
        return airport;
    }

    public async Task<Airport> UpdateAsync(Airport airport, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken);
        var existingIndex = _airports.FindIndex(a => a.Code == airport.Code);
        if (existingIndex >= 0)
        {
            _airports[existingIndex] = airport;
            _logger.LogInformation("Updated airport {Code}", airport.Code);
        }
        return airport;
    }

    public async Task DeleteAsync(string iataCode, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken);
        var airport = _airports.FirstOrDefault(a => a.Code == iataCode);
        if (airport != null)
        {
            _airports.Remove(airport);
            _logger.LogInformation("Deleted airport {Code}", iataCode);
        }
    }    private static List<Airport> InitializeMockAirports()
    {
        return new List<Airport>
        {
            // North America
            new("JFK", "John F. Kennedy International Airport", "New York", "United States", timezone: "America/New_York"),
            new("LAX", "Los Angeles International Airport", "Los Angeles", "United States", timezone: "America/Los_Angeles"),
            new("ORD", "O'Hare International Airport", "Chicago", "United States", timezone: "America/Chicago"),
            new("DFW", "Dallas/Fort Worth International Airport", "Dallas", "United States", timezone: "America/Chicago"),
            new("MIA", "Miami International Airport", "Miami", "United States", timezone: "America/New_York"),
            new("SFO", "San Francisco International Airport", "San Francisco", "United States", timezone: "America/Los_Angeles"),
            new("BOS", "Logan International Airport", "Boston", "United States", timezone: "America/New_York"),
            new("SEA", "Seattle-Tacoma International Airport", "Seattle", "United States", timezone: "America/Los_Angeles"),
            new("ATL", "Hartsfield-Jackson Atlanta International Airport", "Atlanta", "United States", timezone: "America/New_York"),
            new("DEN", "Denver International Airport", "Denver", "United States", timezone: "America/Denver"),
            new("LAS", "McCarran International Airport", "Las Vegas", "United States", timezone: "America/Los_Angeles"),
            new("PHX", "Phoenix Sky Harbor International Airport", "Phoenix", "United States", timezone: "America/Phoenix"),
            new("IAH", "George Bush Intercontinental Airport", "Houston", "United States", timezone: "America/Chicago"),
            new("MSP", "Minneapolis-Saint Paul International Airport", "Minneapolis", "United States", timezone: "America/Chicago"),
            new("YYZ", "Toronto Pearson International Airport", "Toronto", "Canada", timezone: "America/Toronto"),
            new("YVR", "Vancouver International Airport", "Vancouver", "Canada", timezone: "America/Vancouver"),
            
            // Europe
            new("LHR", "London Heathrow Airport", "London", "United Kingdom", timezone: "Europe/London"),
            new("CDG", "Charles de Gaulle Airport", "Paris", "France", timezone: "Europe/Paris"),
            new("FRA", "Frankfurt Airport", "Frankfurt", "Germany", timezone: "Europe/Berlin"),
            new("AMS", "Amsterdam Airport Schiphol", "Amsterdam", "Netherlands", timezone: "Europe/Amsterdam"),
            new("FCO", "Leonardo da Vinci International Airport", "Rome", "Italy", timezone: "Europe/Rome"),
            new("MAD", "Adolfo Suárez Madrid-Barajas Airport", "Madrid", "Spain", timezone: "Europe/Madrid"),
            new("BCN", "Barcelona-El Prat Airport", "Barcelona", "Spain", timezone: "Europe/Madrid"),
            new("ZUR", "Zurich Airport", "Zurich", "Switzerland", timezone: "Europe/Zurich"),
            new("MUC", "Munich Airport", "Munich", "Germany", timezone: "Europe/Berlin"),
            new("VIE", "Vienna International Airport", "Vienna", "Austria", timezone: "Europe/Vienna"),
            
            // Asia-Pacific
            new("NRT", "Narita International Airport", "Tokyo", "Japan", timezone: "Asia/Tokyo"),
            new("HND", "Haneda Airport", "Tokyo", "Japan", timezone: "Asia/Tokyo"),
            new("ICN", "Incheon International Airport", "Seoul", "South Korea", timezone: "Asia/Seoul"),
            new("SIN", "Singapore Changi Airport", "Singapore", "Singapore", timezone: "Asia/Singapore"),
            new("HKG", "Hong Kong International Airport", "Hong Kong", "Hong Kong", timezone: "Asia/Hong_Kong"),
            new("PVG", "Shanghai Pudong International Airport", "Shanghai", "China", timezone: "Asia/Shanghai"),
            new("PEK", "Beijing Capital International Airport", "Beijing", "China", timezone: "Asia/Shanghai"),
            new("BOM", "Chhatrapati Shivaji Maharaj International Airport", "Mumbai", "India", timezone: "Asia/Kolkata"),
            new("DEL", "Indira Gandhi International Airport", "Delhi", "India", timezone: "Asia/Kolkata"),
            new("SYD", "Sydney Kingsford Smith Airport", "Sydney", "Australia", timezone: "Australia/Sydney"),
            new("MEL", "Melbourne Airport", "Melbourne", "Australia", timezone: "Australia/Melbourne"),
            
            // Middle East & Africa
            new("DXB", "Dubai International Airport", "Dubai", "United Arab Emirates", timezone: "Asia/Dubai"),
            new("DOH", "Hamad International Airport", "Doha", "Qatar", timezone: "Asia/Qatar"),
            new("CAI", "Cairo International Airport", "Cairo", "Egypt", timezone: "Africa/Cairo"),
            new("JNB", "O.R. Tambo International Airport", "Johannesburg", "South Africa", timezone: "Africa/Johannesburg"),
            
            // South America
            new("GIG", "Rio de Janeiro-Galeão International Airport", "Rio de Janeiro", "Brazil", timezone: "America/Sao_Paulo"),
            new("GRU", "São Paulo-Guarulhos International Airport", "São Paulo", "Brazil", timezone: "America/Sao_Paulo"),
            new("BSB", "Brasília International Airport", "Brasília", "Brazil", timezone: "America/Sao_Paulo"),
            new("EZE", "Ezeiza International Airport", "Buenos Aires", "Argentina", timezone: "America/Argentina/Buenos_Aires"),
            new("SCL", "Santiago International Airport", "Santiago", "Chile", timezone: "America/Santiago"),
            new("LIM", "Jorge Chávez International Airport", "Lima", "Peru", timezone: "America/Lima"),
            new("BOG", "El Dorado International Airport", "Bogotá", "Colombia", timezone: "America/Bogota")
        };
    }
}
