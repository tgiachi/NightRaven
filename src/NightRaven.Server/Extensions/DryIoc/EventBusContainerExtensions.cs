using DryIoc;
using NightRaven.Hosting.Data;
using NightRaven.Hosting.Interfaces.EventHandlers;
using NightRaven.Hosting.Interfaces.Events;
using NightRaven.Hosting.Interfaces.Services;
using NightRaven.Server.Services.EventBus;
using NightRaven.Server.Services.GameLoop;

namespace NightRaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for the NightRaven event bus + game loop.
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
    /// NightRaven hosting orchestrator.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    public static IContainer AddNightRavenEventBus(this IContainer container)
    {
        container.AddNightRavenHosting();

        container.RegisterConfigSection("game_loop", () => new GameLoopConfig());

        container.AddNightRavenService<IEventBusService, EventBusService>(EventBusPriority);
        container.AddNightRavenService<IGameLoopService, GameLoopService>(GameLoopPriority);

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
