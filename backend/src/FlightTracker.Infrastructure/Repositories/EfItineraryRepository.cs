using FlightTracker.Domain.Entities;
using FlightTracker.Domain.Enums;
using FlightTracker.Domain.Repositories;
using FlightTracker.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlightTracker.Infrastructure.Repositories;

/// <summary>
/// EF Core repository for itineraries (basic persistence + search projection).
/// </summary>
public class EfItineraryRepository : IItineraryRepository
{
    private readonly FlightDbContext _context;
    private readonly ILogger<EfItineraryRepository> _logger;

    public EfItineraryRepository(FlightDbContext context, ILogger<EfItineraryRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Itinerary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Itineraries
            .Include(i => i.Legs)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Itinerary>> SearchAsync(string originCode, string destinationCode, DateTime departureDate, DateTime? returnDate, ItinerarySearchOptions? options = null, CancellationToken cancellationToken = default)
    {
        options ??= ItinerarySearchOptions.Default;

        var depDate = departureDate.Date;
        var retDate = returnDate?.Date;

        var query = _context.Itineraries
            .Include(i => i.Legs)
            .AsQueryable();

        // Filter by origin/destination using first/last leg
        query = query.Where(i => i.Legs.Any(l => l.Sequence == 0 && l.OriginCode == originCode && l.DestinationCode == destinationCode));
        // departure date filter on outbound leg
        query = query.Where(i => i.Legs.Any(l => l.Sequence == 0 && l.DepartureUtc.Date == depDate));

        if (retDate.HasValue)
        {
            // ensure return leg exists with date
            query = query.Where(i => i.Legs.Any(l => l.Direction == LegDirection.Return && l.DepartureUtc.Date == retDate.Value));
        }

        // Sorting
        if (options.SortBy == FlightSortBy.Price)
        {
            query = options.SortOrder == SortOrder.Ascending
                ? query.OrderBy(i => i.TotalPrice.Amount)
                : query.OrderByDescending(i => i.TotalPrice.Amount);
        }
        else if (options.SortBy == FlightSortBy.Duration)
        {
            query = options.SortOrder == SortOrder.Ascending
                ? query.OrderBy(i => i.Legs.Max(l => l.ArrivalUtc) - i.Legs.Min(l => l.DepartureUtc))
                : query.OrderByDescending(i => i.Legs.Max(l => l.ArrivalUtc) - i.Legs.Min(l => l.DepartureUtc));
        }

        var skip = options.Skip;
        var pageSize = options.PageSize;
        var results = await query.Skip(skip).Take(pageSize).ToListAsync(cancellationToken);
        return results.AsReadOnly();
    }

    public async Task<Itinerary> AddAsync(Itinerary itinerary, CancellationToken cancellationToken = default)
    {
        _context.Itineraries.Add(itinerary);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Stored itinerary {ItineraryId} with {LegCount} legs and total {Total} {Currency}", itinerary.Id, itinerary.Legs.Count, itinerary.TotalPrice.Amount, itinerary.TotalPrice.Currency);
        return itinerary;
    }

    public async Task AddRangeAsync(IEnumerable<Itinerary> itineraries, CancellationToken cancellationToken = default)
    {
        _context.Itineraries.AddRange(itineraries);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
