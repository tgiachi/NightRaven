using NightHeaven.Hosting.Data.Metrics;
using NightHeaven.Hosting.Data.Persistence;
using NightHeaven.Hosting.Interfaces.Metrics;
using NightHeaven.Hosting.Types.Metrics;
using NightHeaven.Persistence.Data;
using NightHeaven.Persistence.Interfaces.Persistence;
using NightHeaven.Persistence.Internal;
using NightHeaven.Persistence.Types;
using Serilog;
using ILogger = Serilog.ILogger;

namespace NightHeaven.Persistence.Services.Persistence;

/// <summary>
/// Default <see cref="IPersistenceService" />: builds the registry from boot registrations, performs
/// snapshot load + journal replay, exposes data access, and reports metrics.
/// </summary>
public sealed class PersistenceService : IPersistenceService, IMetricProvider, IDisposable
{
    private readonly ILogger _logger = Log.ForContext<PersistenceService>();
    private readonly PersistenceStateStore _stateStore = new();
    private readonly PersistenceEntityRegistry _registry = new();
    private readonly BinaryJournalService _journal;
    private readonly MessagePackSnapshotService _snapshot;
    private readonly PersistenceConfig _config;
    private readonly IReadOnlyList<PersistenceEntityRegistration> _registrations;

    private long _snapshotsWritten;
    private long _lastSnapshotUnixMilliseconds;
    private int _autosaveInFlight;

    public PersistenceService(
        string saveDirectory,
        PersistenceConfig config,
        IReadOnlyList<PersistenceEntityRegistration> registrations
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(saveDirectory);
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(registrations);

        _config = config;
        _registrations = registrations;
        _journal = new(Path.Combine(saveDirectory, config.JournalFileName), config.EnableFileLock);
        _snapshot = new(Path.Combine(saveDirectory, config.SnapshotFileName));
    }

    public string Prefix => "persistence";

    public IReadOnlyList<MetricSample> Collect()
    {
        long entities;
        long lastSequenceId;

        lock (_stateStore.SyncRoot)
        {
            entities = _registry.GetRegisteredDescriptors().Sum(d => (long)Applier(d.TypeId).Count(_stateStore));
            lastSequenceId = _stateStore.LastSequenceId;
        }

        return
        [
            new("entities_total", entities, Help: "Total persisted entities across types"),
            new("last_sequence_id", lastSequenceId, Help: "Last applied journal sequence id"),
            new(
                "snapshots_written_total",
                Interlocked.Read(ref _snapshotsWritten),
                MetricType.Counter,
                Help: "Total snapshots written"
            ),
            new("last_snapshot_unixms", Interlocked.Read(ref _lastSnapshotUnixMilliseconds), Help: "Last snapshot time")
        ];
    }

    public void Dispose()
    {
        _journal.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _snapshot.Dispose();
    }

    public IDataAccess<TEntity, TKey> GetDataAccess<TEntity, TKey>()
        where TKey : notnull
        => new GenericDataAccess<TEntity, TKey>(_stateStore, _journal, _registry.GetDescriptor<TEntity, TKey>());

    public async ValueTask InitializeAsync(CancellationToken cancellationToken = default)
    {
        var snapshot = await _snapshot.LoadAsync(cancellationToken);

        lock (_stateStore.SyncRoot)
        {
            _stateStore.ClearBuckets();
            _stateStore.LastSequenceId = 0;

            if (snapshot is not null)
            {
                foreach (var bucket in snapshot.EntityBuckets)
                {
                    Applier(bucket.TypeId).LoadBucket(_stateStore, bucket);
                }

                _stateStore.LastSequenceId = snapshot.LastSequenceId;
            }
        }

        var entries = await _journal.ReadAllAsync(cancellationToken);

        lock (_stateStore.SyncRoot)
        {
            foreach (var entry in entries.OrderBy(e => e.SequenceId))
            {
                ApplyEntryLocked(entry);

                if (entry.SequenceId > _stateStore.LastSequenceId)
                {
                    _stateStore.LastSequenceId = entry.SequenceId;
                }
            }
        }

        _logger.Information("Persistence initialized LastSequenceId={LastSequenceId}", _stateStore.LastSequenceId);
    }

    public async ValueTask SaveSnapshotAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _autosaveInFlight, 1) == 1)
        {
            return;
        }

        try
        {
            WorldSnapshot snapshot;

            lock (_stateStore.SyncRoot)
            {
                var buckets = _registry.GetRegisteredDescriptors()
                                       .Select(d => Applier(d.TypeId).CaptureBucket(_stateStore))
                                       .Where(b => b is not null)
                                       .Select(b => b!)
                                       .ToArray();

                snapshot = new()
                {
                    CreatedUnixMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    LastSequenceId = _stateStore.LastSequenceId,
                    EntityBuckets = buckets
                };
            }

            await _snapshot.SaveAsync(snapshot, cancellationToken);
            await _journal.TrimThroughSequenceAsync(snapshot.LastSequenceId, cancellationToken);

            Interlocked.Increment(ref _snapshotsWritten);
            Interlocked.Exchange(ref _lastSnapshotUnixMilliseconds, snapshot.CreatedUnixMilliseconds);
            _logger.Information("Snapshot written LastSequenceId={LastSequenceId}", snapshot.LastSequenceId);
        }
        finally
        {
            Interlocked.Exchange(ref _autosaveInFlight, 0);
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var registration in _registrations)
        {
            RegisterDescriptor(registration.Descriptor);
        }

        _registry.Freeze();
        await InitializeAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
        => await SaveSnapshotAsync(cancellationToken);

    /// <summary>Test/diagnostic hook: stops without writing a snapshot (forces journal-only recovery).</summary>
    public ValueTask StopWithoutSnapshotAsync()
        => ValueTask.CompletedTask;

    private IInternalEntityApplier Applier(ushort typeId)
    {
        if (!_registry.IsRegistered(typeId))
        {
            throw new InvalidOperationException($"No persistence descriptor registered for type id {typeId}.");
        }

        return (IInternalEntityApplier)_registry.GetDescriptor(typeId);
    }

    private void ApplyEntryLocked(JournalEntry entry)
    {
        var applier = Applier(entry.TypeId);

        switch (entry.Operation)
        {
            case JournalEntityOperationType.Upsert:
                applier.ApplyUpsert(_stateStore, entry.Payload);

                break;
            case JournalEntityOperationType.Remove:
                applier.ApplyRemove(_stateStore, entry.Payload);

                break;
            default:
                throw new InvalidOperationException($"Unknown journal operation {entry.Operation}.");
        }
    }

    private void RegisterDescriptor(IPersistenceEntityDescriptor descriptor)
    {
        var method = typeof(PersistenceEntityRegistry).GetMethod(nameof(PersistenceEntityRegistry.Register))!
                                                      .MakeGenericMethod(descriptor.EntityType, descriptor.KeyType);
        method.Invoke(_registry, [descriptor]);
    }
}
