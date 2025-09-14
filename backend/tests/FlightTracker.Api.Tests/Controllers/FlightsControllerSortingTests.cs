using FlightTracker.Api.Application.Queries;
using FlightTracker.Api.Controllers;
using FlightTracker.Api.Application.DTOs;
using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Enums;
using FlightTracker.Domain.ValueObjects;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FlightTracker.Api.Tests.Controllers;

public class FlightsControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<FlightsController>> _loggerMock;
    private readonly FlightsController _controller;

    public FlightsControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<FlightsController>>();
        _controller = new FlightsController(_mediatorMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task SearchFlights_WithValidRequest_ShouldReturnFlights()
    {
        // Arrange
        var origin = "LAX";
        var destination = "JFK";
        var departureDate = DateTime.UtcNow.Date.AddDays(1);
        var sortBy = "price";
        var sortOrder = "desc";
        var page = 1;
        var pageSize = 20;

        var mockResult = new SearchFlightsResult(new List<Flight>(), DateTime.UtcNow);
        _mediatorMock.Setup(m => m.Send(It.IsAny<SearchFlightsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResult);

        // Act
        var result = await _controller.SearchFlights(
            origin, destination, departureDate, null, null, 1, 0, 0, sortBy, sortOrder, page, pageSize);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeOfType<SearchFlightsResult>();

        // Verify the query was sent with correct parameters
        _mediatorMock.Verify(m => m.Send(
            It.Is<SearchFlightsQuery>(q => 
                q.OriginCode == origin &&
                q.DestinationCode == destination &&
                q.DepartureDate == departureDate &&
                q.SearchOptions != null &&
                q.SearchOptions.SortBy == FlightSortBy.Price &&
                q.SearchOptions.SortOrder == SortOrder.Descending &&
                q.SearchOptions.Page == page &&
                q.SearchOptions.PageSize == pageSize),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchFlights_WithoutSortingParameters_ShouldUseDefaults()
    {
        // Arrange
        var origin = "LAX";
        var destination = "JFK";
        var departureDate = DateTime.UtcNow.Date.AddDays(1);

        var mockResult = new SearchFlightsResult(new List<Flight>(), DateTime.UtcNow);
        _mediatorMock.Setup(m => m.Send(It.IsAny<SearchFlightsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResult);

        // Act
        var result = await _controller.SearchFlights(origin, destination, departureDate);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        // Verify the query was sent with default sorting parameters
        _mediatorMock.Verify(m => m.Send(
            It.Is<SearchFlightsQuery>(q => 
                q.SearchOptions != null &&
                q.SearchOptions.SortBy == FlightSortBy.DepartureTime &&
                q.SearchOptions.SortOrder == SortOrder.Ascending &&
                q.SearchOptions.Page == 1 &&
                q.SearchOptions.PageSize == 20),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("departureTime", FlightSortBy.DepartureTime)]
    [InlineData("arrivalTime", FlightSortBy.ArrivalTime)]
    [InlineData("duration", FlightSortBy.Duration)]
    [InlineData("price", FlightSortBy.Price)]
    [InlineData("airline", FlightSortBy.Airline)]
    [InlineData("DEPARTURETIME", FlightSortBy.DepartureTime)] // Case insensitive
    [InlineData("invalid", FlightSortBy.DepartureTime)] // Invalid defaults to DepartureTime
    public async Task SearchFlights_WithDifferentSortBy_ShouldParseCorrectly(string sortByString, FlightSortBy expectedSortBy)
    {
        // Arrange
        var origin = "LAX";
        var destination = "JFK";
        var departureDate = DateTime.UtcNow.Date.AddDays(1);

        var mockResult = new SearchFlightsResult(new List<Flight>(), DateTime.UtcNow);
        _mediatorMock.Setup(m => m.Send(It.IsAny<SearchFlightsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResult);

        // Act
        var result = await _controller.SearchFlights(origin, destination, departureDate, null, null, 1, 0, 0, sortByString);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        // Verify the query was sent with correct sort by parameter
        _mediatorMock.Verify(m => m.Send(
            It.Is<SearchFlightsQuery>(q => 
                q.SearchOptions != null &&
                q.SearchOptions.SortBy == expectedSortBy),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("asc", SortOrder.Ascending)]
    [InlineData("desc", SortOrder.Descending)]
    [InlineData("ASC", SortOrder.Ascending)] // Case insensitive
    [InlineData("invalid", SortOrder.Ascending)] // Invalid defaults to Ascending
    public async Task SearchFlights_WithDifferentSortOrder_ShouldParseCorrectly(string sortOrderString, SortOrder expectedSortOrder)
    {
        // Arrange
        var origin = "LAX";
        var destination = "JFK";
        var departureDate = DateTime.UtcNow.Date.AddDays(1);

        var mockResult = new SearchFlightsResult(new List<Flight>(), DateTime.UtcNow);
        _mediatorMock.Setup(m => m.Send(It.IsAny<SearchFlightsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResult);

        // Act
        var result = await _controller.SearchFlights(origin, destination, departureDate, null, null, 1, 0, 0, "price", sortOrderString);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        // Verify the query was sent with correct sort order parameter
        _mediatorMock.Verify(m => m.Send(
            It.Is<SearchFlightsQuery>(q => 
                q.SearchOptions != null &&
                q.SearchOptions.SortOrder == expectedSortOrder),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0, 0, 1, 1)] // Page <1 ->1, size <1 ->1
    [InlineData(1, 101, 1, 100)] // Cap size at 100
    [InlineData(2, 50, 2, 50)] // Valid
    public async Task SearchFlights_WithDifferentPagination_ShouldHandleCorrectly(
        int inputPage, int inputPageSize, int expectedPage, int expectedPageSize)
    {
        var origin = "LAX";
        var destination = "JFK";
        var departureDate = DateTime.UtcNow.Date.AddDays(1);

        var mockResult = new SearchFlightsResult(new List<Flight>(), DateTime.UtcNow);
        _mediatorMock.Setup(m => m.Send(It.IsAny<SearchFlightsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockResult);

        var result = await _controller.SearchFlights(origin, destination, departureDate, null, null, 1, 0, 0, "price", "asc", inputPage, inputPageSize);
        result.Should().BeOfType<OkObjectResult>();
        _mediatorMock.Verify(m => m.Send(
            It.Is<SearchFlightsQuery>(q => q.SearchOptions != null && q.SearchOptions.Page == expectedPage && q.SearchOptions.PageSize == expectedPageSize),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchFlights_WhenMediatorThrows_ShouldReturn500()
    {
        var origin = "LAX";
        var destination = "JFK";
        var departureDate = DateTime.UtcNow.Date.AddDays(1);
        _mediatorMock.Setup(m => m.Send(It.IsAny<SearchFlightsQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));
        var result = await _controller.SearchFlights(origin, destination, departureDate);
        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
    }
}
