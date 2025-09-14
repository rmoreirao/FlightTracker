using FlightTracker.Api.Application.DTOs;using FlightTracker.Api.Application.Mapping;using FlightTracker.Api.Application.Queries;using FlightTracker.Domain.Services;using MediatR;using Microsoft.Extensions.Logging;

namespace FlightTracker.Api.Application.Handlers;

public class SearchItinerariesHandler : IRequestHandler<SearchItinerariesQuery, SearchItinerariesResult>
{
    private readonly IItinerarySearchService _service; private readonly ILogger<SearchItinerariesHandler> _logger;
    public SearchItinerariesHandler(IItinerarySearchService service, ILogger<SearchItinerariesHandler> logger){_service=service;_logger=logger;}

    public async Task<SearchItinerariesResult> Handle(SearchItinerariesQuery request, CancellationToken cancellationToken)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        var itineraries = await _service.SearchAsync(request.OriginCode, request.DestinationCode, request.DepartureDate, request.ReturnDate, request.Options, cancellationToken);
        sw.Stop();
        var list = itineraries.Select(i=>i.ToDto()).ToList();
        _logger.LogInformation("Itinerary search {Origin}-{Destination} dep {Dep} ret {Ret} returned {Count} in {Ms}ms", request.OriginCode, request.DestinationCode, request.DepartureDate.ToString("yyyy-MM-dd"), request.ReturnDate?.ToString("yyyy-MM-dd") ?? "-", list.Count, sw.ElapsedMilliseconds);
        return new SearchItinerariesResult{ Items=list, Page=request.Options.Page, PageSize=request.Options.PageSize, Returned=list.Count, SortBy=request.Options.SortBy.ToString(), SortOrder=request.Options.SortOrder.ToString(), RoundTripRequested=request.ReturnDate.HasValue};
    }
}
