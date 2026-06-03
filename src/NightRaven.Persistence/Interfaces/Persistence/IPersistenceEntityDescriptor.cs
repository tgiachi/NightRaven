namespace NightRaven.Persistence.Interfaces.Persistence;

/// <summary>
/// Describes a registered persisted entity type.
/// </summary>
public interface IPersistenceEntityDescriptor
{
    /// <summary>Stable numeric identifier for the persisted entity kind.</summary>
    ushort TypeId { get; }

    /// <summary>Stable diagnostic name for the persisted entity kind.</summary>
    string TypeName { get; }

    /// <summary>Version of the persisted entity schema.</summary>
    int SchemaVersion { get; }

    /// <summary>CLR type of the entity.</summary>
    Type EntityType { get; }

    /// <summary>CLR type of the entity key.</summary>
    Type KeyType { get; }
}

/// <summary>
/// Strongly typed descriptor for a persisted entity kind.
/// </summary>
public interface IPersistenceEntityDescriptor<TEntity, TKey> : IPersistenceEntityDescriptor
{
    /// <summary>Creates a detached clone of the entity.</summary>
    TEntity Clone(TEntity entity);

    /// <summary>Deserializes a snapshot bucket.</summary>
    IReadOnlyList<TEntity> DeserializeBucket(byte[] payload);

    /// <summary>Deserializes one entity from a journal payload.</summary>
    TEntity DeserializeEntity(byte[] payload);

    /// <summary>Deserializes a key from a journal payload.</summary>
    TKey DeserializeKey(byte[] payload);

    /// <summary>Gets the entity key.</summary>
    TKey GetKey(TEntity entity);

    /// <summary>Serializes a snapshot bucket.</summary>
    byte[] SerializeBucket(IReadOnlyCollection<TEntity> entities);

    /// <summary>Serializes one entity for a journal upsert.</summary>
    byte[] SerializeEntity(TEntity entity);

    /// <summary>Serializes a key for a journal removal.</summary>
    byte[] SerializeKey(TKey key);
}
