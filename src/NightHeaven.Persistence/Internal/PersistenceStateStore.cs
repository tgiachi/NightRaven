namespace NightHeaven.Persistence.Internal;

/// <summary>
/// In-memory mutable world state shared by persistence data-access instances. Not thread-safe by
/// itself; callers synchronize on <see cref="SyncRoot" />.
/// </summary>
internal sealed class PersistenceStateStore
{
    private readonly Dictionary<ushort, object> _entityBuckets = [];

    public object SyncRoot { get; } = new();

    public long LastSequenceId { get; set; }

    public Dictionary<TKey, TEntity> GetBucket<TEntity, TKey>(ushort typeId)
        where TKey : notnull
    {
        if (_entityBuckets.TryGetValue(typeId, out var existing))
        {
            return (Dictionary<TKey, TEntity>)existing;
        }

        var created = new Dictionary<TKey, TEntity>();
        _entityBuckets[typeId] = created;

        return created;
    }

    public void ClearBuckets()
        => _entityBuckets.Clear();
}
