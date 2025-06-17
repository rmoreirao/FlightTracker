namespace FlightTracker.Domain.ValueObjects;

/// <summary>
/// Value object representing a date range for flight searches
/// </summary>
public class DateRange : IEquatable<DateRange>
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }
    public int Days => (EndDate - StartDate).Days + 1;

    public DateRange(DateTime startDate, DateTime endDate)
    {
        if (startDate.Date > endDate.Date)
            throw new ArgumentException("Start date cannot be after end date");

        StartDate = startDate.Date;
        EndDate = endDate.Date;
    }

    public static DateRange SingleDay(DateTime date) => new(date, date);

    public static DateRange FromDeparture(DateTime departureDate, DateTime? returnDate = null)
    {
        return returnDate.HasValue 
            ? new DateRange(departureDate, returnDate.Value)
            : SingleDay(departureDate);
    }

    public bool Contains(DateTime date)
    {
        var dateOnly = date.Date;
        return dateOnly >= StartDate && dateOnly <= EndDate;
    }

    public bool Overlaps(DateRange other)
    {
        if (other == null) return false;
        return StartDate <= other.EndDate && EndDate >= other.StartDate;
    }

    public bool Equals(DateRange? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return StartDate.Equals(other.StartDate) && EndDate.Equals(other.EndDate);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is DateRange other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(StartDate, EndDate);
    }

    public static bool operator ==(DateRange? left, DateRange? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(DateRange? left, DateRange? right)
    {
        return !Equals(left, right);
    }

    public override string ToString()
    {
        return StartDate == EndDate 
            ? StartDate.ToString("yyyy-MM-dd")
            : $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
    }
}
