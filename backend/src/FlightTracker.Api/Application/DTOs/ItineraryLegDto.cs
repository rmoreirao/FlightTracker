namespace FlightTracker.Api.Application.DTOs;

public sealed record ItineraryLegDto(
    int Sequence,
    string FlightNumber,
    string AirlineCode,
    string Origin,
    string Destination,
    DateTime DepartureUtc,
    DateTime ArrivalUtc,
    int DurationMinutes,
    string CabinClass,
    decimal PriceAmount,
    string PriceCurrency,
    string Direction);
