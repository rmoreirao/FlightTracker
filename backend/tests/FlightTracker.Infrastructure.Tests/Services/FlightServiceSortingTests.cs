using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Enums;
using FlightTracker.Domain.ValueObjects;
using FlightTracker.Infrastructure.Repositories;
using FlightTracker.Infrastructure.Services;
using FlightTracker.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FlightTracker.Infrastructure.Tests.Services;

public class FlightServiceSortingTests
{
    private readonly Mock<IFlightRepository> _flightRepositoryMock;
    private readonly Mock<IFlightQueryRepository> _flightQueryRepositoryMock;
    private readonly Mock<ILogger<FlightService>> _loggerMock;
    private readonly FlightService _service;

    public FlightServiceSortingTests()
    {
        _flightRepositoryMock = new Mock<IFlightRepository>();
        _flightQueryRepositoryMock = new Mock<IFlightQueryRepository>();
        _loggerMock = new Mock<ILogger<FlightService>>();
        _service = new FlightService(
            _flightRepositoryMock.Object,
            _flightQueryRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SearchFlightsAsync_WithSortingOptions_ShouldPassToRepository()
    {
        // Arrange
        var originCode = "LAX";
        var destinationCode = "JFK";
        var departureDate = DateTime.UtcNow.Date.AddDays(1);
        var searchOptions = FlightSearchOptions.Create(
            FlightSortBy.Price,
            SortOrder.Ascending,
            1,
            20);

        var mockFlights = new List<Flight>().AsReadOnly();
    _flightRepositoryMock.Setup(r => r.SearchAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<DateTime>(),
        It.IsAny<DateTime?>(),
        It.IsAny<FlightSearchOptions?>(),
        It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockFlights);

        // Act
        var result = await _service.SearchFlightsAsync(
            originCode, destinationCode, departureDate, null, searchOptions);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(mockFlights);

        _flightRepositoryMock.Verify(r => r.SearchAsync(
            originCode,
            destinationCode,
            departureDate,
            null,
            searchOptions,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchFlightsAsync_WithoutSortingOptions_ShouldPassNullToRepository()
    {
        // Arrange
        var originCode = "LAX";
        var destinationCode = "JFK";
        var departureDate = DateTime.UtcNow.Date.AddDays(1);

        var mockFlights = new List<Flight>().AsReadOnly();
    _flightRepositoryMock.Setup(r => r.SearchAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<DateTime>(),
        It.IsAny<DateTime?>(),
        It.IsAny<FlightSearchOptions?>(),
        It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockFlights);

        // Act
    var result = await _service.SearchFlightsAsync(originCode, destinationCode, departureDate, null);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(mockFlights);

        _flightRepositoryMock.Verify(r => r.SearchAsync(
            originCode,
            destinationCode,
            departureDate,
            null,
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchFlightsAsync_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var originCode = "LAX";
        var destinationCode = "JFK";
        var departureDate = DateTime.UtcNow.Date.AddDays(1);

    _flightRepositoryMock.Setup(r => r.SearchAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<DateTime>(),
        It.IsAny<DateTime?>(),
        It.IsAny<FlightSearchOptions?>(),
        It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Repository error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SearchFlightsAsync(originCode, destinationCode, departureDate, null));
    }

    [Fact]
    public async Task SearchFlightsAsync_ShouldLogSearchOperation()
    {
        // Arrange
        var originCode = "LAX";
        var destinationCode = "JFK";
        var departureDate = DateTime.UtcNow.Date.AddDays(1);

        var mockFlights = new List<Flight>().AsReadOnly();
    _flightRepositoryMock.Setup(r => r.SearchAsync(
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<DateTime>(),
        It.IsAny<DateTime?>(),
        It.IsAny<FlightSearchOptions?>(),
        It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockFlights);

        // Act
    await _service.SearchFlightsAsync(originCode, destinationCode, departureDate, null);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Searching flights from {originCode} to {destinationCode}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Found {mockFlights.Count} outbound flights")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
