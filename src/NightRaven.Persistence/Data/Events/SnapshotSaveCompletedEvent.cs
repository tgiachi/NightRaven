using NightRaven.Abstractions.Interfaces.Events;

namespace NightRaven.Persistence.Data.Events;

/// <summary>
/// Async event published after a persistence snapshot completes successfully.
/// </summary>
public sealed record SnapshotSaveCompletedEvent(
    long LastSequenceId,
    int EntityBucketCount,
    DateTimeOffset StartedAt,
    DateTimeOffset At
) : IAsyncEvent;
