using FlightTracker.Domain.Entities;
using FlightTracker.Domain.ValueObjects;

namespace FlightTracker.Domain.Services;

public interface IItinerarySearchService
{
    Task<IReadOnlyList<Itinerary>> SearchAsync(
        string originCode,
        string destinationCode,
        DateTime departureDate,
        DateTime? returnDate,
        ItinerarySearchOptions? options = null,
        CancellationToken cancellationToken = default);
}
