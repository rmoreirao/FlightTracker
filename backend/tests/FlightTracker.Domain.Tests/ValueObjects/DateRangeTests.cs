using FlightTracker.Domain.ValueObjects;
using FlightTracker.Domain.Tests.Base;

namespace FlightTracker.Domain.Tests.ValueObjects;

public class DateRangeTests : ValueObjectTestBase<DateRange>
{
    protected override DateRange CreateValidValueObject() => 
        new(DateTime.Today, DateTime.Today.AddDays(7));

    protected override DateRange CreateDifferentValueObject() => 
        new(DateTime.Today.AddDays(1), DateTime.Today.AddDays(10));

    [Fact]
    public void Constructor_WithValidDates_ShouldCreateDateRange()
    {
        // Arrange
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(7);

        // Act
        var dateRange = new DateRange(startDate, endDate);

        // Assert
        dateRange.StartDate.Should().Be(startDate);
        dateRange.EndDate.Should().Be(endDate);
    }

    [Fact]
    public void Constructor_WithEndDateBeforeStartDate_ShouldThrowArgumentException()
    {
        // Arrange
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(-1);

        // Act
        var act = () => new DateRange(startDate, endDate);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("End date must be greater than or equal to start date*");
    }

    [Fact]
    public void Constructor_WithSameDates_ShouldCreateValidDateRange()
    {
        // Arrange
        var date = DateTime.Today;

        // Act
        var dateRange = new DateRange(date, date);

        // Assert
        dateRange.StartDate.Should().Be(date);
        dateRange.EndDate.Should().Be(date);
        dateRange.Duration.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Duration_ShouldCalculateCorrectTimeSpan()
    {
        // Arrange
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(5);
        var dateRange = new DateRange(startDate, endDate);

        // Act
        var duration = dateRange.Duration;

        // Assert
        duration.Should().Be(TimeSpan.FromDays(5));
    }

    [Fact]
    public void DurationInDays_ShouldReturnCorrectNumberOfDays()
    {
        // Arrange
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(10);
        var dateRange = new DateRange(startDate, endDate);

        // Act
        var days = dateRange.DurationInDays;

        // Assert
        days.Should().Be(10);
    }

    [Fact]
    public void Contains_WithDateWithinRange_ShouldReturnTrue()
    {
        // Arrange
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(10);
        var dateRange = new DateRange(startDate, endDate);
        var dateInRange = DateTime.Today.AddDays(5);

        // Act
        var result = dateRange.Contains(dateInRange);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithStartDate_ShouldReturnTrue()
    {
        // Arrange
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(10);
        var dateRange = new DateRange(startDate, endDate);

        // Act
        var result = dateRange.Contains(startDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithEndDate_ShouldReturnTrue()
    {
        // Arrange
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(10);
        var dateRange = new DateRange(startDate, endDate);

        // Act
        var result = dateRange.Contains(endDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithDateBeforeRange_ShouldReturnFalse()
    {
        // Arrange
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(10);
        var dateRange = new DateRange(startDate, endDate);
        var dateBeforeRange = DateTime.Today.AddDays(-1);

        // Act
        var result = dateRange.Contains(dateBeforeRange);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_WithDateAfterRange_ShouldReturnFalse()
    {
        // Arrange
        var startDate = DateTime.Today;
        var endDate = DateTime.Today.AddDays(10);
        var dateRange = new DateRange(startDate, endDate);
        var dateAfterRange = DateTime.Today.AddDays(11);

        // Act
        var result = dateRange.Contains(dateAfterRange);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Overlaps_WithOverlappingRange_ShouldReturnTrue()
    {
        // Arrange
        var range1 = new DateRange(DateTime.Today, DateTime.Today.AddDays(10));
        var range2 = new DateRange(DateTime.Today.AddDays(5), DateTime.Today.AddDays(15));

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_WithTouchingRanges_ShouldReturnTrue()
    {
        // Arrange
        var range1 = new DateRange(DateTime.Today, DateTime.Today.AddDays(10));
        var range2 = new DateRange(DateTime.Today.AddDays(10), DateTime.Today.AddDays(20));

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_WithNonOverlappingRange_ShouldReturnFalse()
    {
        // Arrange
        var range1 = new DateRange(DateTime.Today, DateTime.Today.AddDays(10));
        var range2 = new DateRange(DateTime.Today.AddDays(11), DateTime.Today.AddDays(20));

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Overlaps_WithContainedRange_ShouldReturnTrue()
    {
        // Arrange
        var outerRange = new DateRange(DateTime.Today, DateTime.Today.AddDays(20));
        var innerRange = new DateRange(DateTime.Today.AddDays(5), DateTime.Today.AddDays(15));

        // Act
        var result = outerRange.Overlaps(innerRange);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsContainedBy_WithContainingRange_ShouldReturnTrue()
    {
        // Arrange
        var innerRange = new DateRange(DateTime.Today.AddDays(5), DateTime.Today.AddDays(15));
        var outerRange = new DateRange(DateTime.Today, DateTime.Today.AddDays(20));

        // Act
        var result = innerRange.IsContainedBy(outerRange);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsContainedBy_WithNonContainingRange_ShouldReturnFalse()
    {
        // Arrange
        var range1 = new DateRange(DateTime.Today, DateTime.Today.AddDays(15));
        var range2 = new DateRange(DateTime.Today.AddDays(5), DateTime.Today.AddDays(10));

        // Act
        var result = range1.IsContainedBy(range2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsContainedBy_WithSameRange_ShouldReturnTrue()
    {
        // Arrange
        var range1 = new DateRange(DateTime.Today, DateTime.Today.AddDays(10));
        var range2 = new DateRange(DateTime.Today, DateTime.Today.AddDays(10));

        // Act
        var result = range1.IsContainedBy(range2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Extend_WithPositiveDays_ShouldExtendEndDate()
    {
        // Arrange
        var originalRange = new DateRange(DateTime.Today, DateTime.Today.AddDays(10));

        // Act
        var extendedRange = originalRange.Extend(5);

        // Assert
        extendedRange.StartDate.Should().Be(originalRange.StartDate);
        extendedRange.EndDate.Should().Be(originalRange.EndDate.AddDays(5));
    }

    [Fact]
    public void Extend_WithNegativeDays_ShouldShortenEndDate()
    {
        // Arrange
        var originalRange = new DateRange(DateTime.Today, DateTime.Today.AddDays(10));

        // Act
        var shortenedRange = originalRange.Extend(-3);

        // Assert
        shortenedRange.StartDate.Should().Be(originalRange.StartDate);
        shortenedRange.EndDate.Should().Be(originalRange.EndDate.AddDays(-3));
    }

    [Fact]
    public void Extend_ThatWouldMakeEndDateBeforeStartDate_ShouldThrowArgumentException()
    {
        // Arrange
        var range = new DateRange(DateTime.Today, DateTime.Today.AddDays(5));

        // Act
        var act = () => range.Extend(-10);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Extension would result in end date before start date*");
    }

    [Fact]
    public void Shift_WithPositiveDays_ShouldShiftBothDatesForward()
    {
        // Arrange
        var originalRange = new DateRange(DateTime.Today, DateTime.Today.AddDays(10));

        // Act
        var shiftedRange = originalRange.Shift(5);

        // Assert
        shiftedRange.StartDate.Should().Be(originalRange.StartDate.AddDays(5));
        shiftedRange.EndDate.Should().Be(originalRange.EndDate.AddDays(5));
        shiftedRange.Duration.Should().Be(originalRange.Duration);
    }

    [Fact]
    public void Shift_WithNegativeDays_ShouldShiftBothDatesBackward()
    {
        // Arrange
        var originalRange = new DateRange(DateTime.Today.AddDays(10), DateTime.Today.AddDays(20));

        // Act
        var shiftedRange = originalRange.Shift(-5);

        // Assert
        shiftedRange.StartDate.Should().Be(originalRange.StartDate.AddDays(-5));
        shiftedRange.EndDate.Should().Be(originalRange.EndDate.AddDays(-5));
        shiftedRange.Duration.Should().Be(originalRange.Duration);
    }

    [Theory]
    [AutoData]
    public void Constructor_WithAutoFixtureData_ShouldCreateValidDateRange(int dayOffset)
    {
        // Arrange
        dayOffset = Math.Abs(dayOffset) % 365; // Ensure positive and reasonable offset
        var startDate = DateTime.Today;
        var endDate = startDate.AddDays(dayOffset);

        // Act
        var dateRange = new DateRange(startDate, endDate);

        // Assert
        dateRange.StartDate.Should().Be(startDate);
        dateRange.EndDate.Should().Be(endDate);
        dateRange.DurationInDays.Should().Be(dayOffset);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 15);
        var endDate = new DateTime(2024, 1, 20);
        var dateRange = new DateRange(startDate, endDate);

        // Act
        var result = dateRange.ToString();

        // Assert
        result.Should().Be("2024-01-15 to 2024-01-20");
    }

    [Fact]
    public void CreateSingleDay_ShouldCreateDateRangeForSingleDay()
    {
        // Arrange
        var date = DateTime.Today;

        // Act
        var dateRange = DateRange.CreateSingleDay(date);

        // Assert
        dateRange.StartDate.Should().Be(date);
        dateRange.EndDate.Should().Be(date);
        dateRange.Duration.Should().Be(TimeSpan.Zero);
        dateRange.DurationInDays.Should().Be(0);
    }

    [Fact]
    public void CreateWeek_ShouldCreateSevenDayRange()
    {
        // Arrange
        var startDate = DateTime.Today;

        // Act
        var dateRange = DateRange.CreateWeek(startDate);

        // Assert
        dateRange.StartDate.Should().Be(startDate);
        dateRange.EndDate.Should().Be(startDate.AddDays(6));
        dateRange.DurationInDays.Should().Be(6);
    }

    [Fact]
    public void CreateMonth_ShouldCreateMonthRange()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);

        // Act
        var dateRange = DateRange.CreateMonth(startDate);

        // Assert
        dateRange.StartDate.Should().Be(startDate);
        dateRange.EndDate.Should().Be(new DateTime(2024, 1, 31));
        dateRange.DurationInDays.Should().Be(30); // January has 31 days, so 30 days duration
    }
}
