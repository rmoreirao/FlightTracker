using FlightTracker.Domain.Enums;
using FlightTracker.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace FlightTracker.Domain.Tests.ValueObjects;

public class FlightSearchOptionsTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldSucceed()
    {
        // Arrange & Act
        var options = FlightSearchOptions.Create(
            sortBy: FlightSortBy.Price,
            sortOrder: SortOrder.Ascending,
            page: 1,
            pageSize: 20);

        // Assert
        options.SortBy.Should().Be(FlightSortBy.Price);
        options.SortOrder.Should().Be(SortOrder.Ascending);
        options.Page.Should().Be(1);
        options.PageSize.Should().Be(20);
    }

    [Fact]
    public void Default_ShouldUseDefaultValues()
    {
        // Arrange & Act
        var options = FlightSearchOptions.Default;

        // Assert
        options.SortBy.Should().Be(FlightSortBy.DepartureTime);
        options.SortOrder.Should().Be(SortOrder.Ascending);
        options.Page.Should().Be(1);
        options.PageSize.Should().Be(20);
    }

    [Fact]
    public void Create_WithInvalidPage_ShouldUseMinimumValue()
    {
        // Arrange & Act
        var options = FlightSearchOptions.Create(page: 0);

        // Assert
        options.Page.Should().Be(1);
    }

    [Fact]
    public void Create_WithInvalidPageSize_ShouldUseMinimumOrMaximumValue()
    {
        // Arrange & Act
        var optionsMin = FlightSearchOptions.Create(pageSize: 0);
        var optionsMax = FlightSearchOptions.Create(pageSize: 101);

        // Assert
        optionsMin.PageSize.Should().Be(1);
        optionsMax.PageSize.Should().Be(100);
    }

    [Fact]
    public void WithSorting_ShouldCreateOptionsWithSorting()
    {
        // Arrange & Act
        var options = FlightSearchOptions.WithSorting(FlightSortBy.Duration, SortOrder.Descending);

        // Assert
        options.SortBy.Should().Be(FlightSortBy.Duration);
        options.SortOrder.Should().Be(SortOrder.Descending);
        options.Page.Should().Be(1);
        options.PageSize.Should().Be(20);
    }

    [Fact]
    public void WithPagination_ShouldCreateOptionsWithPagination()
    {
        // Arrange & Act
        var options = FlightSearchOptions.WithPagination(2, 10);

        // Assert
        options.Page.Should().Be(2);
        options.PageSize.Should().Be(10);
        options.SortBy.Should().Be(FlightSortBy.DepartureTime);
        options.SortOrder.Should().Be(SortOrder.Ascending);
    }

    [Fact]
    public void Skip_ShouldCalculateCorrectSkipValue()
    {
        // Arrange
        var options = FlightSearchOptions.Create(page: 3, pageSize: 10);

        // Act
        var skip = options.Skip;

        // Assert
        skip.Should().Be(20); // (3-1) * 10
    }

    [Fact]
    public void IsValid_WithValidOptions_ShouldReturnTrue()
    {
        // Arrange
        var options = FlightSearchOptions.Create(page: 1, pageSize: 20);

        // Act & Assert
        options.IsValid().Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnMeaningfulString()
    {
        // Arrange
        var options = FlightSearchOptions.Create(
            FlightSortBy.Price, SortOrder.Descending, 2, 15);

        // Act
        var result = options.ToString();

        // Assert
        result.Should().Contain("Price");
        result.Should().Contain("Descending");
        result.Should().Contain("2");
        result.Should().Contain("15");
    }
}
