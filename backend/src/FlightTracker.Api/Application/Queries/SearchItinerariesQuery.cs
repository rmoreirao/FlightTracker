using FlightTracker.Api.Application.DTOs;using FlightTracker.Domain.Enums;using FlightTracker.Domain.ValueObjects;using MediatR;

namespace FlightTracker.Api.Application.Queries;

public record SearchItinerariesQuery(
    string OriginCode,
    string DestinationCode,
    DateTime DepartureDate,
    DateTime? ReturnDate,
    ItinerarySearchOptions Options) : IRequest<SearchItinerariesResult>;
