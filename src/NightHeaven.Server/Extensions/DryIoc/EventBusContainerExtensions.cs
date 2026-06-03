using DryIoc;
using NightHeaven.Hosting.Data;
using NightHeaven.Hosting.Interfaces.EventHandlers;
using NightHeaven.Hosting.Interfaces.Events;
using NightHeaven.Hosting.Interfaces.Services;
using NightHeaven.Server.Services.EventBus;
using NightHeaven.Server.Services.GameLoop;

namespace NightHeaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for the NightHeaven event bus + game loop.
/// </summary>
public static class EventBusContainerExtensions
{
    private const int EventBusPriority = 0;
    private const int GameLoopPriority = 10;

    /// <summary>
    /// Registers <typeparamref name="THandler" /> as an <see cref="IAsyncEventHandler{TEvent}" /> singleton.
    /// </summary>
    public static IContainer AddAsyncEventHandler<THandler, TEvent>(this IContainer container)
        where THandler : class, IAsyncEventHandler<TEvent>
        where TEvent : IAsyncEvent
    {
        container.Register<THandler>(Reuse.Singleton);
        container.RegisterMapping<IAsyncEventHandler<TEvent>, THandler>();

        return container;
    }

    /// <summary>
    /// Registers <see cref="EventBusService" /> and <see cref="GameLoopService" /> with the
    /// NightHeaven hosting orchestrator.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    /// <param name="configure">Optional callback to customize <see cref="GameLoopConfig" />.</param>
    public static IContainer AddNightHeavenEventBus(
        this IContainer container,
        Action<GameLoopConfig>? configure = null
    )
    {
        container.AddNightHeavenHosting();

        var config = new GameLoopConfig();
        configure?.Invoke(config);
        container.RegisterInstance(config);

        container.AddNightHeavenService<IEventBusService, EventBusService>(EventBusPriority);
        container.AddNightHeavenService<IGameLoopService, GameLoopService>(GameLoopPriority);

        return container;
    }

    /// <summary>
    /// Registers <typeparamref name="THandler" /> as an <see cref="ITickEventHandler{TEvent}" /> singleton.
    /// </summary>
    public static IContainer AddTickEventHandler<THandler, TEvent>(this IContainer container)
        where THandler : class, ITickEventHandler<TEvent>
        where TEvent : ITickEvent
    {
        container.Register<THandler>(Reuse.Singleton);
        container.RegisterMapping<ITickEventHandler<TEvent>, THandler>();

        return container;
    }
}
