using DryIoc;

namespace NightHeaven.Scripting.Lua.Extensions.Container;

/// <summary>
/// Extension methods for registering typed lists in the DryIoc container.
/// </summary>
public static class AddTypedListMethodExtension
{
    /// <summary>
    /// Adds an entity to a typed list in the DryIoc container.
    /// If the list doesn't exist, it creates and registers a new one.
    /// </summary>
    /// <typeparam name="TListEntity">The type of entities in the list.</typeparam>
    /// <param name="container">The DryIoc container.</param>
    /// <param name="entity">The entity to add to the list.</param>
    /// <returns>The same container for chaining.</returns>
    public static IContainer AddToRegisterTypedList<TListEntity>(this IContainer container, TListEntity entity)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(entity);

        if (container.IsRegistered<List<TListEntity>>())
        {
            var typedList = container.Resolve<List<TListEntity>>();
            typedList.Add(entity);
        }
        else
        {
            var typedList = new List<TListEntity> { entity };
            container.RegisterInstance(typedList);
        }

        return container;
    }
}
