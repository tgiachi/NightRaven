using NightHeaven.Persistence.Data;

namespace NightHeaven.Persistence.Internal;

/// <summary>
/// Type-erased hook implemented by the typed descriptor so the persistence service can apply journal
/// mutations and capture/load snapshot buckets without knowing the entity/key generic arguments.
/// </summary>
internal interface IInternalEntityApplier
{
    void ApplyUpsert(PersistenceStateStore stateStore, byte[] payload);
    void ApplyRemove(PersistenceStateStore stateStore, byte[] payload);
    EntitySnapshotBucket? CaptureBucket(PersistenceStateStore stateStore);
    void LoadBucket(PersistenceStateStore stateStore, EntitySnapshotBucket bucket);
    int Count(PersistenceStateStore stateStore);
}
