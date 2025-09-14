using FlightTracker.Domain.Entities;
using FlightTracker.Domain.ValueObjects;

namespace FlightTracker.Domain.Repositories;

/// <summary>
/// Repository abstraction for storing and retrieving itineraries.
/// </summary>
public interface IItineraryRepository
{
    Task<Itinerary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Itinerary>> SearchAsync(
        string originCode,
        string destinationCode,
        DateTime departureDate,
        DateTime? returnDate,
        ItinerarySearchOptions? options = null,
        CancellationToken cancellationToken = default);
    Task<Itinerary> AddAsync(Itinerary itinerary, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Itinerary> itineraries, CancellationToken cancellationToken = default);
}
