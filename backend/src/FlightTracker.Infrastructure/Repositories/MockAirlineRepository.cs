using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Infrastructure.Repositories;

/// <summary>
/// Mock implementation of airline repository for development
/// </summary>
public class MockAirlineRepository : IAirlineRepository
{
    private readonly ILogger<MockAirlineRepository> _logger;
    private readonly List<Airline> _airlines;

    public MockAirlineRepository(ILogger<MockAirlineRepository> logger)
    {
        _logger = logger;
        _airlines = InitializeMockAirlines();
    }    public async Task<Airline?> GetByCodeAsync(string iataCode, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken);
        return _airlines.FirstOrDefault(a => a.Code == iataCode);
    }

    public async Task<IReadOnlyList<Airline>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        var lowerSearchTerm = searchTerm.ToLowerInvariant();

        return _airlines.Where(a =>
            a.Code.Contains(lowerSearchTerm, StringComparison.OrdinalIgnoreCase) ||
            a.Name.Contains(lowerSearchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    public async Task<IReadOnlyList<Airline>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await Task.Delay(100, cancellationToken);
        return _airlines.AsReadOnly();
    }

    public async Task<Airline> AddAsync(Airline airline, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken);
        _airlines.Add(airline);
        _logger.LogInformation("Added airline {Code}", airline.Code);
        return airline;
    }

    public async Task<Airline> UpdateAsync(Airline airline, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken);
        var existingIndex = _airlines.FindIndex(a => a.Code == airline.Code);
        if (existingIndex >= 0)
        {
            _airlines[existingIndex] = airline;
            _logger.LogInformation("Updated airline {Code}", airline.Code);
        }
        return airline;
    }

    public async Task DeleteAsync(string iataCode, CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken);
        var airline = _airlines.FirstOrDefault(a => a.Code == iataCode);
        if (airline != null)
        {
            _airlines.Remove(airline);
            _logger.LogInformation("Deleted airline {Code}", iataCode);
        }
    }    private static List<Airline> InitializeMockAirlines()
    {
        return new List<Airline>
        {
            new("AA", "American Airlines"),
            new("DL", "Delta Air Lines"),
            new("UA", "United Airlines"),
            new("WN", "Southwest Airlines"),
            new("B6", "JetBlue Airways"),
            new("AS", "Alaska Airlines"),
            new("NK", "Spirit Airlines"),
            new("F9", "Frontier Airlines"),
            new("AC", "Air Canada"),
            new("WS", "WestJet"),
            new("BA", "British Airways"),
            new("VS", "Virgin Atlantic"),
            new("AF", "Air France"),
            new("KL", "KLM Royal Dutch Airlines"),
            new("LH", "Lufthansa"),
            new("LX", "Swiss International Air Lines"),
            new("OS", "Austrian Airlines"),
            new("IB", "Iberia"),
            new("AZ", "Alitalia"),
            new("SN", "Brussels Airlines"),
            new("SK", "Scandinavian Airlines"),
            new("AY", "Finnair"),
            new("JL", "Japan Airlines"),
            new("NH", "All Nippon Airways"),
            new("SQ", "Singapore Airlines"),
            new("CX", "Cathay Pacific"),
            new("QF", "Qantas"),
            new("EK", "Emirates"),
            new("QR", "Qatar Airways"),
            new("EY", "Etihad Airways"),
            new("TK", "Turkish Airlines"),
            new("KE", "Korean Air"),
            new("OZ", "Asiana Airlines"),
            new("CI", "China Airlines"),
            new("BR", "EVA Air")
        };
    }
}
