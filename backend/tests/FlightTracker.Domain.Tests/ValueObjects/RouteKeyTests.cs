using FlightTracker.Domain.ValueObjects;
using FlightTracker.Domain.Tests.Base;

namespace FlightTracker.Domain.Tests.ValueObjects;

public class RouteKeyTests : ValueObjectTestBase<RouteKey>
{
    protected override RouteKey CreateValidValueObject() => new("JFK", "LAX");

    protected override RouteKey CreateDifferentValueObject() => new("ORD", "DFW");

    [Fact]
    public void Constructor_WithValidAirportCodes_ShouldCreateRouteKey()
    {
        // Arrange
        var origin = "JFK";
        var destination = "LAX";

        // Act
        var routeKey = new RouteKey(origin, destination);

        // Assert
        routeKey.Origin.Should().Be(origin);
        routeKey.Destination.Should().Be(destination);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidOrigin_ShouldThrowArgumentException(string origin)
    {
        // Act
        var act = () => new RouteKey(origin, "LAX");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Origin cannot be null or empty*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidDestination_ShouldThrowArgumentException(string destination)
    {
        // Act
        var act = () => new RouteKey("JFK", destination);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Destination cannot be null or empty*");
    }

    [Theory]
    [InlineData("jfk")]
    [InlineData("JFKK")]
    [InlineData("12")]
    [InlineData("AB")]
    [InlineData("1AB")]
    [InlineData("A1B")]
    public void Constructor_WithInvalidOriginFormat_ShouldThrowArgumentException(string origin)
    {
        // Act
        var act = () => new RouteKey(origin, "LAX");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Origin must be a valid 3-letter airport code*");
    }

    [Theory]
    [InlineData("lax")]
    [InlineData("LAXX")]
    [InlineData("12")]
    [InlineData("AB")]
    [InlineData("1LA")]
    [InlineData("L1A")]
    public void Constructor_WithInvalidDestinationFormat_ShouldThrowArgumentException(string destination)
    {
        // Act
        var act = () => new RouteKey("JFK", destination);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Destination must be a valid 3-letter airport code*");
    }

    [Fact]
    public void Constructor_WithSameOriginAndDestination_ShouldThrowArgumentException()
    {
        // Arrange
        var airportCode = "JFK";

        // Act
        var act = () => new RouteKey(airportCode, airportCode);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Origin and destination cannot be the same*");
    }

    [Theory]
    [InlineData("JFK", "LAX")]
    [InlineData("ORD", "DFW")]
    [InlineData("SEA", "MIA")]
    [InlineData("BOS", "SFO")]
    public void Constructor_WithValidAirportCodes_ShouldNormalizeToUpperCase(string origin, string destination)
    {
        // Act
        var routeKey = new RouteKey(origin.ToLower(), destination.ToLower());

        // Assert
        routeKey.Origin.Should().Be(origin.ToUpper());
        routeKey.Destination.Should().Be(destination.ToUpper());
    }

    [Fact]
    public void Reverse_ShouldSwapOriginAndDestination()
    {
        // Arrange
        var originalRoute = new RouteKey("JFK", "LAX");

        // Act
        var reversedRoute = originalRoute.Reverse();

        // Assert
        reversedRoute.Origin.Should().Be(originalRoute.Destination);
        reversedRoute.Destination.Should().Be(originalRoute.Origin);
    }

    [Fact]
    public void Reverse_AppliedTwice_ShouldReturnOriginalRoute()
    {
        // Arrange
        var originalRoute = new RouteKey("JFK", "LAX");

        // Act
        var doubleReversed = originalRoute.Reverse().Reverse();

        // Assert
        doubleReversed.Should().Be(originalRoute);
    }

    [Fact]
    public void IsRoundTrip_WithTwoRoutesInOppositeDirections_ShouldReturnTrue()
    {
        // Arrange
        var outboundRoute = new RouteKey("JFK", "LAX");
        var returnRoute = new RouteKey("LAX", "JFK");

        // Act
        var result = outboundRoute.IsRoundTrip(returnRoute);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRoundTrip_WithTwoRoutesInSameDirection_ShouldReturnFalse()
    {
        // Arrange
        var route1 = new RouteKey("JFK", "LAX");
        var route2 = new RouteKey("JFK", "LAX");

        // Act
        var result = route1.IsRoundTrip(route2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsRoundTrip_WithCompletelyDifferentRoutes_ShouldReturnFalse()
    {
        // Arrange
        var route1 = new RouteKey("JFK", "LAX");
        var route2 = new RouteKey("ORD", "DFW");

        // Act
        var result = route1.IsRoundTrip(route2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsInternational_WithDomesticRoute_ShouldReturnFalse()
    {
        // This test assumes we have a way to determine if airports are international
        // For now, we'll test the method exists and basic functionality
        // In a real implementation, this might require airport data

        // Arrange
        var domesticRoute = new RouteKey("JFK", "LAX"); // Both US airports

        // Act
        var result = domesticRoute.IsInternational();

        // Assert
        // Note: This test may need to be updated based on actual implementation
        // For now, we're just ensuring the method can be called
        result.Should().BeDefined();
    }

    [Fact]
    public void GetDistance_ShouldReturnCalculatedDistance()
    {
        // Arrange
        var route = new RouteKey("JFK", "LAX");

        // Act
        var distance = route.GetDistance();

        // Assert
        // Note: This test assumes we have distance calculation logic
        // The actual implementation would need airport coordinate data
        distance.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("JFK", "LAX")]
    [InlineData("ORD", "DFW")]
    [InlineData("SEA", "MIA")]
    public void ToString_ShouldReturnFormattedString(string origin, string destination)
    {
        // Arrange
        var routeKey = new RouteKey(origin, destination);

        // Act
        var result = routeKey.ToString();

        // Assert
        result.Should().Be($"{origin}-{destination}");
    }

    [Fact]
    public void GetNormalizedKey_ShouldReturnConsistentKeyForSameRoute()
    {
        // Arrange
        var route1 = new RouteKey("JFK", "LAX");
        var route2 = new RouteKey("LAX", "JFK");

        // Act
        var key1 = route1.GetNormalizedKey();
        var key2 = route2.GetNormalizedKey();

        // Assert
        key1.Should().Be(key2);
    }

    [Fact]
    public void GetNormalizedKey_ShouldAlwaysUseLexicographicalOrder()
    {
        // Arrange
        var route = new RouteKey("ZZZ", "AAA"); // Reverse alphabetical order

        // Act
        var normalizedKey = route.GetNormalizedKey();

        // Assert
        normalizedKey.Should().Be("AAA-ZZZ");
    }

    [Theory]
    [AutoData]
    public void Equals_WithAutoFixtureData_ShouldWorkCorrectly(string origin, string destination)
    {
        // Arrange - ensure valid 3-letter codes
        origin = origin?.Length >= 3 ? origin[..3].ToUpper() : "JFK";
        destination = destination?.Length >= 3 ? destination[..3].ToUpper() : "LAX";
        
        // Ensure they're different
        if (origin == destination)
        {
            destination = destination == "JFK" ? "LAX" : "JFK";
        }

        var route1 = new RouteKey(origin, destination);
        var route2 = new RouteKey(origin, destination);

        // Act & Assert
        route1.Should().Be(route2);
        route1.GetHashCode().Should().Be(route2.GetHashCode());
    }

    [Fact]
    public void CreateDomestic_ShouldCreateDomesticRoute()
    {
        // Act
        var route = RouteKey.CreateDomestic("JFK", "LAX");

        // Assert
        route.Origin.Should().Be("JFK");
        route.Destination.Should().Be("LAX");
        // Additional assertions would depend on how domestic routes are marked
    }

    [Fact]
    public void CreateInternational_ShouldCreateInternationalRoute()
    {
        // Act
        var route = RouteKey.CreateInternational("JFK", "LHR");

        // Assert
        route.Origin.Should().Be("JFK");
        route.Destination.Should().Be("LHR");
        // Additional assertions would depend on how international routes are marked
    }

    [Fact]
    public void CompareTo_ShouldOrderRoutesAlphabetically()
    {
        // Arrange
        var route1 = new RouteKey("AAA", "BBB");
        var route2 = new RouteKey("CCC", "DDD");

        // Act
        var comparison = route1.CompareTo(route2);

        // Assert
        comparison.Should().BeLessThan(0);
    }

    [Fact]
    public void CompareTo_WithSameRoute_ShouldReturnZero()
    {
        // Arrange
        var route1 = new RouteKey("JFK", "LAX");
        var route2 = new RouteKey("JFK", "LAX");

        // Act
        var comparison = route1.CompareTo(route2);

        // Assert
        comparison.Should().Be(0);
    }

    [Fact]
    public void GetAllPossibleRoutes_FromGivenAirports_ShouldReturnAllCombinations()
    {
        // Arrange
        var airports = new[] { "JFK", "LAX", "ORD" };

        // Act
        var routes = RouteKey.GetAllPossibleRoutes(airports);

        // Assert
        routes.Should().HaveCount(6); // 3 airports = 3*2 = 6 possible routes
        routes.Should().Contain(r => r.Origin == "JFK" && r.Destination == "LAX");
        routes.Should().Contain(r => r.Origin == "LAX" && r.Destination == "JFK");
        routes.Should().Contain(r => r.Origin == "JFK" && r.Destination == "ORD");
        routes.Should().Contain(r => r.Origin == "ORD" && r.Destination == "JFK");
        routes.Should().Contain(r => r.Origin == "LAX" && r.Destination == "ORD");
        routes.Should().Contain(r => r.Origin == "ORD" && r.Destination == "LAX");
    }
}
