namespace NightRaven.Core.Interfaces.Ids;

/// <summary>
/// Typed auto-increment key. Implement on custom ID structs to gain automatic serial allocation
/// in <see cref="NightRaven.Persistence.Interfaces.Persistence.IAutoDataAccess{TEntity,TKey}" />.
/// </summary>
public interface IAutoIncrementKey<TSelf> : IAutoIncrementKey
    where TSelf : struct, IAutoIncrementKey<TSelf>
{
    /// <summary>Creates a key from the given sequence value.</summary>
    abstract static TSelf FromSequence(ulong value);
}
