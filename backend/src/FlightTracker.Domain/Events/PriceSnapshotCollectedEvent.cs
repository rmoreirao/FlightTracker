using FlightTracker.Domain.Entities;

namespace FlightTracker.Domain.Events;

/// <summary>
/// Event raised when a price snapshot is collected
/// </summary>
public class PriceSnapshotCollectedEvent : DomainEvent
{
    public PriceSnapshot Snapshot { get; }
    public string ProviderName { get; }

    public PriceSnapshotCollectedEvent(PriceSnapshot snapshot, string providerName)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        ProviderName = providerName ?? throw new ArgumentNullException(nameof(providerName));
    }
}
