using FlightTracker.Api.Application.DTOs;
using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Enums;

namespace FlightTracker.Api.Application.Mapping;

public static class ItineraryMappings
{
    public static ItineraryDto ToDto(this Itinerary itinerary)
    {
        return new ItineraryDto(
            itinerary.Id,
            itinerary.Origin,
            itinerary.FinalDestination,
            itinerary.IsRoundTrip,
            itinerary.OutboundDeparture,
            itinerary.ReturnDeparture,
            itinerary.TotalPrice.Amount,
            itinerary.TotalPrice.Currency,
            (int)itinerary.TotalDuration.TotalMinutes,
            itinerary.Legs.Count,
            itinerary.Legs.Select(l => l.ToDto()).ToList());
    }

    public static ItineraryLegDto ToDto(this ItineraryLeg leg)
    {
        return new ItineraryLegDto(
            leg.Sequence,
            leg.FlightNumber,
            leg.AirlineCode,
            leg.OriginCode,
            leg.DestinationCode,
            leg.DepartureUtc,
            leg.ArrivalUtc,
            (int)leg.Duration.TotalMinutes,
            leg.CabinClass.ToString(),
            leg.PriceComponent.Amount,
            leg.PriceComponent.Currency,
            leg.Direction.ToString());
    }
}
