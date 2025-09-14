using FlightTracker.Domain.Enums;
using FlightTracker.Domain.ValueObjects;

namespace FlightTracker.Domain.Entities;

/// <summary>
/// Aggregate representing a travel itinerary (one-way, round-trip, or future multi-city).
/// </summary>
public class Itinerary
{
    private readonly List<ItineraryLeg> _legs = new();
    public Guid Id { get; private set; } = Guid.NewGuid();
    public IReadOnlyList<ItineraryLeg> Legs => _legs.AsReadOnly();

    public Money TotalPrice { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public string Origin => _legs.Count == 0 ? string.Empty : _legs.First().OriginCode;
    public string FinalDestination => _legs.Count == 0 ? string.Empty : _legs.Last().DestinationCode;
    public DateTime? OutboundDeparture => _legs.FirstOrDefault(l => l.Direction == LegDirection.Outbound)?.DepartureUtc;
    public DateTime? ReturnDeparture => _legs.FirstOrDefault(l => l.Direction == LegDirection.Return)?.DepartureUtc;
    public bool IsRoundTrip => _legs.Count(l => l.Direction == LegDirection.Return) == 1 && _legs.Count(l => l.Direction == LegDirection.Outbound) >= 1;
    public TimeSpan TotalDuration => _legs.Count == 0 ? TimeSpan.Zero : _legs.Last().ArrivalUtc - _legs.First().DepartureUtc;

    private Itinerary() {}

    private Itinerary(IEnumerable<ItineraryLeg> legs)
    {
        _legs = legs.OrderBy(l => l.Sequence).ToList();
        // Ensure all legs reference this aggregate Id
        foreach (var leg in _legs)
        {
            leg.ItineraryId = Id;
        }
        ValidateSequential();
        ValidateTemporal();
        ValidateDirections();
        TotalPrice = CalculateTotalPrice();
    }

    public static Itinerary Create(IEnumerable<ItineraryLeg> legs)
    {
        if (legs == null) throw new ArgumentNullException(nameof(legs));
        var list = legs.ToList();
        if (!list.Any()) throw new ArgumentException("Itinerary must have at least one leg");
        return new Itinerary(list);
    }

    private void ValidateSequential()
    {
        for (int i = 0; i < _legs.Count; i++)
        {
            if (_legs[i].Sequence != i) throw new InvalidOperationException("Leg sequence must be contiguous starting at 0");
        }
    }

    private void ValidateTemporal()
    {
        for (int i = 0; i < _legs.Count - 1; i++)
        {
            if (_legs[i + 1].DepartureUtc < _legs[i].ArrivalUtc)
                throw new InvalidOperationException("Legs must not overlap and must be ordered by time");
        }
    }

    private void ValidateDirections()
    {
        // Basic round-trip rule: if there's a Return leg ensure final destination loops back to origin
        if (IsRoundTrip && Origin != FinalDestination)
            throw new InvalidOperationException("Round-trip itinerary must end where it started");
    }

    private Money CalculateTotalPrice()
    {
        var first = _legs.First().PriceComponent;
        decimal total = 0m;
        foreach (var l in _legs)
        {
            if (l.PriceComponent.Currency != first.Currency)
                throw new InvalidOperationException("All legs must share currency");
            total += l.PriceComponent.Amount;
        }
        return new Money(total, first.Currency);
    }
}
