using FlightTracker.Api.Application.DTOs;
using FlightTracker.Api.Application.Queries;
using FlightTracker.Api.Controllers;
using FlightTracker.Domain.Enums;
using FlightTracker.Domain.ValueObjects;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FlightTracker.Api.Tests.Controllers;

public class ItinerariesControllerTests
{
    private readonly Mock<IMediator> _mediator;
    private readonly Mock<ILogger<ItinerariesController>> _logger;
    private readonly ItinerariesController _controller;

    public ItinerariesControllerTests()
    {
        _mediator = new Mock<IMediator>();
        _logger = new Mock<ILogger<ItinerariesController>>();
        _controller = new ItinerariesController(_mediator.Object, _logger.Object);
    }

    [Fact]
    public async Task Search_WithValidParams_SendsQueryAndReturnsOk()
    {
        var origin = "LAX";
        var destination = "JFK";
        var departure = DateTime.UtcNow.Date.AddDays(2);
    var resultDto = new SearchItinerariesResult { Items = Array.Empty<ItineraryDto>(), Page=2, PageSize=15, Returned=0, SortBy=nameof(FlightSortBy.Price), SortOrder=nameof(SortOrder.Descending), RoundTripRequested=false };
        _mediator.Setup(m => m.Send(It.IsAny<SearchItinerariesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultDto);

        var action = await _controller.Search(origin, destination, departure, null, 2, 15, 10, 12, 120, "price", "desc");

        action.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeSameAs(resultDto);

        _mediator.Verify(m => m.Send(It.Is<SearchItinerariesQuery>(q =>
            q.OriginCode == origin &&
            q.DestinationCode == destination &&
            q.DepartureDate == departure &&
            q.Options.SortBy == FlightSortBy.Price &&
            q.Options.SortOrder == SortOrder.Descending &&
            q.Options.Page == 2 &&
            q.Options.PageSize == 15 &&
            q.Options.MaxOutboundFlights == 10 &&
            q.Options.MaxReturnFlights == 12 &&
            q.Options.MaxCombinations == 120
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("price", FlightSortBy.Price)]
    [InlineData("duration", FlightSortBy.Duration)]
    [InlineData("invalid", FlightSortBy.Price)]
    public async Task Search_SortByParsing_Works(string sortBy, FlightSortBy expected)
    {
    var resultDto = new SearchItinerariesResult { Items = Array.Empty<ItineraryDto>(), Page=1, PageSize=20, Returned=0, SortBy=nameof(FlightSortBy.Price), SortOrder=nameof(SortOrder.Ascending), RoundTripRequested=false };
        _mediator.Setup(m => m.Send(It.IsAny<SearchItinerariesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultDto);

        var action = await _controller.Search("LAX", "JFK", DateTime.UtcNow.Date.AddDays(1), null, 1, 20, 40, 40, 400, sortBy, null);
        action.Should().BeOfType<OkObjectResult>();
        _mediator.Verify(m => m.Send(It.Is<SearchItinerariesQuery>(q => q.Options.SortBy == expected), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("asc", SortOrder.Ascending)]
    [InlineData("desc", SortOrder.Descending)]
    [InlineData("other", SortOrder.Ascending)]
    public async Task Search_SortOrderParsing_Works(string sortOrder, SortOrder expected)
    {
    var resultDto = new SearchItinerariesResult { Items = Array.Empty<ItineraryDto>(), Page=1, PageSize=20, Returned=0, SortBy=nameof(FlightSortBy.Price), SortOrder=nameof(SortOrder.Ascending), RoundTripRequested=false };
        _mediator.Setup(m => m.Send(It.IsAny<SearchItinerariesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultDto);

        var action = await _controller.Search("LAX", "JFK", DateTime.UtcNow.Date.AddDays(1), null, 1, 20, 40, 40, 400, "price", sortOrder);
        action.Should().BeOfType<OkObjectResult>();
        _mediator.Verify(m => m.Send(It.Is<SearchItinerariesQuery>(q => q.Options.SortOrder == expected), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0, 0, 1, 1)] // Normalized
    [InlineData(1, 500, 1, 100)] // Cap pageSize at controller normalization (initial cap before factory hard cap)
    [InlineData(3, 50, 3, 50)] // Valid
    public async Task Search_PaginationNormalization_Works(int inputPage, int inputSize, int expectedPage, int expectedSize)
    {
    var resultDto = new SearchItinerariesResult { Items = Array.Empty<ItineraryDto>(), Page=expectedPage, PageSize=expectedSize, Returned=0, SortBy=nameof(FlightSortBy.Price), SortOrder=nameof(SortOrder.Ascending), RoundTripRequested=false };
        _mediator.Setup(m => m.Send(It.IsAny<SearchItinerariesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultDto);

        var action = await _controller.Search("LAX", "JFK", DateTime.UtcNow.Date.AddDays(1), null, inputPage, inputSize, 40, 40, 400, null, null);
        action.Should().BeOfType<OkObjectResult>();
        _mediator.Verify(m => m.Send(It.Is<SearchItinerariesQuery>(q => q.Options.Page == expectedPage && q.Options.PageSize == expectedSize), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Search_WhenMediatorThrows_Returns500()
    {
        _mediator.Setup(m => m.Send(It.IsAny<SearchItinerariesQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("boom"));

        var action = await _controller.Search("LAX", "JFK", DateTime.UtcNow.Date.AddDays(1));
        action.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
    }
}
