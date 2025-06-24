using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Enums;
using FlightTracker.Domain.ValueObjects;
using FlightTracker.Domain.Tests.Base;

namespace FlightTracker.Domain.Tests.Entities;

public class FlightTests : DomainTestBase
{
    private Flight CreateValidFlight()
    {
        var origin = new Airport("JFK", "John F. Kennedy International", "New York", "USA");
        var destination = new Airport("LAX", "Los Angeles International", "Los Angeles", "USA");
        var price = new Money(299.99m, "USD");
        var departureTime = DateTime.Today.AddDays(1).AddHours(10);
        var arrivalTime = departureTime.AddHours(6);

        return new Flight(
            "AA1234",
            "AA",
            "American Airlines",
            origin,
            destination,
            departureTime,
            arrivalTime,
            price,
            CabinClass.Economy);
    }    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateFlight()
    {
        // Arrange
        var flightNumber = "AA1234";
        var airlineCode = "AA";
        var airlineName = "American Airlines";
        var origin = new Airport("JFK", "John F. Kennedy International", "New York", "USA");
        var destination = new Airport("LAX", "Los Angeles International", "Los Angeles", "USA");
        var departureTime = DateTime.Today.AddDays(1).AddHours(10);
        var arrivalTime = departureTime.AddHours(6);
        var price = new Money(299.99m, "USD");
        var cabinClass = CabinClass.Economy;

        // Act
        var flight = new Flight(flightNumber, airlineCode, airlineName, origin, destination, 
                               departureTime, arrivalTime, price, cabinClass);

        // Assert
        flight.FlightNumber.Should().Be(flightNumber);
        flight.AirlineCode.Should().Be(airlineCode);
        flight.AirlineName.Should().Be(airlineName);
        flight.Origin.Should().Be(origin);
        flight.Destination.Should().Be(destination);
        flight.DepartureTime.Should().Be(departureTime);
        flight.ArrivalTime.Should().Be(arrivalTime);
        flight.Price.Should().Be(price);
        flight.CabinClass.Should().Be(cabinClass);
        flight.Status.Should().Be(FlightStatus.Scheduled);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidFlightNumber_ShouldThrowArgumentException(string flightNumber)
    {
        // Arrange
        var origin = new Airport("JFK", "John F. Kennedy International", "New York", "USA");
        var destination = new Airport("LAX", "Los Angeles International", "Los Angeles", "USA");
        var price = new Money(299.99m, "USD");
        var departureTime = DateTime.Today.AddDays(1);
        var arrivalTime = departureTime.AddHours(6);

        // Act
        var act = () => new Flight(flightNumber, "AA", "American Airlines", origin, destination,
                                  departureTime, arrivalTime, price, CabinClass.Economy);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Flight number is required*");
    }

    [Fact]
    public void Constructor_WithSameOriginAndDestination_ShouldThrowArgumentException()
    {
        // Arrange
        var airport = new Airport("JFK", "John F. Kennedy International", "New York", "USA");
        var price = new Money(299.99m, "USD");
        var departureTime = DateTime.Today.AddDays(1);
        var arrivalTime = departureTime.AddHours(6);

        // Act
        var act = () => new Flight("AA1234", "AA", "American Airlines", airport, airport,
                                  departureTime, arrivalTime, price, CabinClass.Economy);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Origin and destination cannot be the same*");
    }

    [Fact]
    public void Constructor_WithArrivalTimeBeforeDepartureTime_ShouldThrowArgumentException()
    {
        // Arrange
        var origin = new Airport("JFK", "John F. Kennedy International", "New York", "USA");
        var destination = new Airport("LAX", "Los Angeles International", "Los Angeles", "USA");
        var price = new Money(299.99m, "USD");
        var departureTime = DateTime.Today.AddDays(1).AddHours(10);
        var arrivalTime = departureTime.AddHours(-1); // Before departure

        // Act
        var act = () => new Flight("AA1234", "AA", "American Airlines", origin, destination,
                                  departureTime, arrivalTime, price, CabinClass.Economy);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Arrival time must be after departure time*");
    }

    [Fact]
    public void Duration_ShouldCalculateCorrectTimeSpan()
    {
        // Arrange
        var flight = CreateValidFlight();

        // Act
        var duration = flight.Duration;

        // Assert
        duration.Should().Be(TimeSpan.FromHours(6));
    }    [Fact]
    public void AddSegment_WithValidSegment_ShouldAddToCollection()
    {
        // Arrange
        var flight = CreateValidFlight();
        var segment = new FlightSegment(
            "AA1234",
            "AA",
            new Airport("JFK", "John F. Kennedy International", "New York", "USA"),
            new Airport("LAX", "Los Angeles International", "Los Angeles", "USA"),
            DateTime.Today.AddDays(1).AddHours(10),
            DateTime.Today.AddDays(1).AddHours(16),
            1);

        // Act
        flight.AddSegment(segment);

        // Assert
        flight.Segments.Should().HaveCount(1);
        flight.Segments.Should().Contain(segment);
    }

    [Fact]
    public void AddSegment_WithNullSegment_ShouldThrowArgumentNullException()
    {
        // Arrange
        var flight = CreateValidFlight();

        // Act
        var act = () => flight.AddSegment(null);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("segment");
    }

    [Fact]
    public void UpdatePrice_WithValidPrice_ShouldUpdatePrice()
    {
        // Arrange
        var flight = CreateValidFlight();
        var newPrice = new Money(399.99m, "USD");

        // Act
        flight.UpdatePrice(newPrice);

        // Assert
        flight.Price.Should().Be(newPrice);
    }

    [Fact]
    public void UpdatePrice_WithNullPrice_ShouldThrowArgumentNullException()
    {
        // Arrange
        var flight = CreateValidFlight();

        // Act
        var act = () => flight.UpdatePrice(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UpdateStatus_WithValidStatus_ShouldUpdateStatus()
    {
        // Arrange
        var flight = CreateValidFlight();

        // Act
        flight.UpdateStatus(FlightStatus.InFlight);

        // Assert
        flight.Status.Should().Be(FlightStatus.InFlight);
    }

    [Fact]
    public void IsDirect_WithNoSegments_ShouldReturnTrue()
    {
        // Arrange
        var flight = CreateValidFlight();

        // Act
        var result = flight.IsDirect;

        // Assert
        result.Should().BeTrue();
    }    [Fact]
    public void IsDirect_WithOneSegment_ShouldReturnTrue()
    {
        // Arrange
        var flight = CreateValidFlight();
        var segment = new FlightSegment(
            "AA1234",
            "AA",
            flight.Origin,
            flight.Destination,
            flight.DepartureTime,
            flight.ArrivalTime,
            1);
        flight.AddSegment(segment);

        // Act
        var result = flight.IsDirect;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInternational_WithDomesticFlight_ShouldReturnFalse()
    {
        // Arrange
        var flight = CreateValidFlight(); // Both airports in USA

        // Act
        var result = flight.IsInternational;

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsInternational_WithInternationalFlight_ShouldReturnTrue()
    {
        // Arrange
        var origin = new Airport("JFK", "John F. Kennedy International", "New York", "USA");
        var destination = new Airport("LHR", "London Heathrow", "London", "UK");
        var price = new Money(599.99m, "USD");
        var departureTime = DateTime.Today.AddDays(1).AddHours(10);
        var arrivalTime = departureTime.AddHours(8);

        var flight = new Flight("BA1234", "BA", "British Airways", origin, destination,
                               departureTime, arrivalTime, price, CabinClass.Economy);

        // Act
        var result = flight.IsInternational;

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Stops_WithNoSegments_ShouldReturnZero()
    {
        // Arrange
        var flight = CreateValidFlight();

        // Act
        var stops = flight.Stops;

        // Assert
        stops.Should().Be(0);
    }    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var flight = CreateValidFlight();

        // Act
        var result = flight.ToString();

        // Assert
        result.Should().Contain("AA1234");
        result.Should().Contain("JFK-LAX");
        result.Should().MatchRegex(@"299[.,]99"); // Handle different decimal separators
        result.Should().Contain("USD");
    }

    [Theory]
    [AutoData]
    public void Flight_WithAutoFixtureData_ShouldBeValid(string flightNumber, string airlineCode, 
                                                         string airlineName, decimal priceAmount)
    {
        // Arrange - ensure valid data
        flightNumber = string.IsNullOrEmpty(flightNumber) ? "AA1234" : flightNumber.ToUpper();
        airlineCode = string.IsNullOrEmpty(airlineCode) ? "AA" : airlineCode[..Math.Min(2, airlineCode.Length)].ToUpper();
        airlineName = string.IsNullOrEmpty(airlineName) ? "Test Airlines" : airlineName;
        priceAmount = Math.Abs(priceAmount) % 10000; // Ensure positive and reasonable

        var origin = new Airport("JFK", "John F. Kennedy International", "New York", "USA");
        var destination = new Airport("LAX", "Los Angeles International", "Los Angeles", "USA");
        var price = new Money(priceAmount, "USD");
        var departureTime = DateTime.Today.AddDays(1).AddHours(10);
        var arrivalTime = departureTime.AddHours(6);

        // Act
        var flight = new Flight(flightNumber, airlineCode, airlineName, origin, destination,
                               departureTime, arrivalTime, price, CabinClass.Economy);

        // Assert
        flight.FlightNumber.Should().Be(flightNumber.ToUpper());
        flight.AirlineCode.Should().Be(airlineCode.ToUpper());
        flight.AirlineName.Should().Be(airlineName);
        flight.Price.Amount.Should().Be(priceAmount);
        flight.Duration.Should().Be(TimeSpan.FromHours(6));
    }
}
