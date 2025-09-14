namespace FlightTracker.Api.Application.DTOs;

public sealed record ItineraryDto(
    Guid Id,
    string Origin,
    string FinalDestination,
    bool IsRoundTrip,
    DateTime? OutboundDeparture,
    DateTime? ReturnDeparture,
    decimal TotalPriceAmount,
    string TotalPriceCurrency,
    int TotalDurationMinutes,
    int LegCount,
    IReadOnlyList<ItineraryLegDto> Legs);
