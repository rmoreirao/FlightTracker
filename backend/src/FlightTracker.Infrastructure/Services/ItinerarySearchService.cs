using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Enums;
using FlightTracker.Domain.Repositories;
using FlightTracker.Domain.Services;
using FlightTracker.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Infrastructure.Services;

/// <summary>
/// Builds itineraries by pairing outbound and return flights. Current implementation is in-memory.
/// </summary>
public class ItinerarySearchService : IItinerarySearchService
{
    private readonly IFlightRepository _flightRepository;
    private readonly IItineraryRepository _itineraryRepository;
    private readonly ILogger<ItinerarySearchService> _logger;

    public ItinerarySearchService(IFlightRepository flightRepository, IItineraryRepository itineraryRepository, ILogger<ItinerarySearchService> logger)
    {
        _flightRepository = flightRepository;
        _itineraryRepository = itineraryRepository;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Itinerary>> SearchAsync(string originCode, string destinationCode, DateTime departureDate, DateTime? returnDate, ItinerarySearchOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= ItinerarySearchOptions.Default;

        // One-way: wrap flights individually
        if (!returnDate.HasValue)
        {
            var flights = await _flightRepository.SearchAsync(originCode, destinationCode, departureDate, null, FlightSearchOptions.Default, cancellationToken);
            var itineraries = flights.Select(f => Itinerary.Create(new[] { CreateLegFromFlight(0, f, LegDirection.Outbound) })).ToList();
            return itineraries;
        }

        // Round-trip pairing
        var outboundFlights = await _flightRepository.SearchAsync(originCode, destinationCode, departureDate, null, FlightSearchOptions.Default, cancellationToken);
        var returnFlights = await _flightRepository.SearchAsync(destinationCode, originCode, returnDate.Value, null, FlightSearchOptions.Default, cancellationToken);

        var limitedOutbound = outboundFlights.Take(options.MaxOutboundFlights).ToList();
        var limitedReturn = returnFlights.Take(options.MaxReturnFlights).ToList();

        var results = new List<Itinerary>();
        foreach (var o in limitedOutbound)
        {
            foreach (var r in limitedReturn)
            {
                if (results.Count >= options.MaxCombinations) break;
                if (r.DepartureTime <= o.ArrivalTime) continue; // temporal constraint

                // Stay constraints
                var stay = r.DepartureTime - o.ArrivalTime;
                if (options.MinStay.HasValue && stay < options.MinStay.Value) continue;
                if (options.MaxStay.HasValue && stay > options.MaxStay.Value) continue;

                // Cabin consistency if required
                if (!options.AllowMixedCabin && o.CabinClass != r.CabinClass) continue;

                var legs = new[]
                {
                    CreateLegFromFlight(0, o, LegDirection.Outbound),
                    CreateLegFromFlight(1, r, LegDirection.Return)
                };
                try
                {
                    var itinerary = Itinerary.Create(legs);
                    results.Add(itinerary);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Skipping invalid itinerary combination");
                }
            }
        }

        // Sorting
        results = Sort(results, options).ToList();

        // Pagination
        var paged = results.Skip(options.Skip).Take(options.PageSize).ToList();
        _logger.LogInformation("Generated {Count} itineraries (paged {PagedCount}) for {Origin}-{Destination}", results.Count, paged.Count, originCode, destinationCode);
        return paged;
    }

    private static IEnumerable<Itinerary> Sort(IEnumerable<Itinerary> itineraries, ItinerarySearchOptions options)
    {
        return options.SortBy switch
        {
            FlightSortBy.Price => options.SortOrder == SortOrder.Ascending
                ? itineraries.OrderBy(i => i.TotalPrice.Amount)
                : itineraries.OrderByDescending(i => i.TotalPrice.Amount),
            FlightSortBy.Duration => options.SortOrder == SortOrder.Ascending
                ? itineraries.OrderBy(i => i.TotalDuration)
                : itineraries.OrderByDescending(i => i.TotalDuration),
            _ => itineraries.OrderBy(i => i.TotalPrice.Amount)
        };
    }

    private static ItineraryLeg CreateLegFromFlight(int sequence, Flight flight, LegDirection direction)
    {
        return new ItineraryLeg(sequence, flight.Id, flight.FlightNumber, flight.AirlineCode, flight.Origin!.Code, flight.Destination!.Code,
            flight.DepartureTime, flight.ArrivalTime, flight.Price, flight.CabinClass, direction);
    }
}
