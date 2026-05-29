using Microsoft.Extensions.DependencyInjection;
using NightHeaven.Hosting.Data;
using NightHeaven.Hosting.Extensions;
using NightHeaven.Hosting.Interfaces;
using NightHeaven.Server.Services.EventBus;
using NightHeaven.Server.Services.GameLoop;

namespace NightHeaven.Server.Extensions;

/// <summary>
/// DI registration helpers for the NightHeaven event bus + game loop.
/// </summary>
public static class EventBusServiceCollectionExtensions
{
    private const int EventBusPriority = 0;
    private const int GameLoopPriority = 10;

    /// <summary>
    /// Registers <see cref="EventBusService" /> and <see cref="GameLoopService" /> with the
    /// NightHeaven hosting orchestrator. Calls <see cref="ServiceCollectionExtensions.AddNightHeavenHosting" />
    /// internally (idempotent).
    /// </summary>
    /// <param name="services">DI service collection.</param>
    /// <param name="configure">Optional callback to customize <see cref="GameLoopConfig" />.</param>
    public static IServiceCollection AddNightHeavenEventBus(
        this IServiceCollection services,
        Action<GameLoopConfig>? configure = null
    )
    {
        services.AddNightHeavenHosting();

        var config = new GameLoopConfig();
        configure?.Invoke(config);
        services.AddSingleton(config);

        services.AddNightHeavenService<IEventBusService, EventBusService>(EventBusPriority);
        services.AddNightHeavenService<IGameLoopService, GameLoopService>(GameLoopPriority);

        return services;
    }

    /// <summary>
    /// Registers <typeparamref name="THandler" /> as an <see cref="IAsyncEventHandler{TEvent}" /> singleton.
    /// </summary>
    public static IServiceCollection AddAsyncEventHandler<THandler, TEvent>(this IServiceCollection services)
        where THandler : class, IAsyncEventHandler<TEvent>
        where TEvent : IAsyncEvent
    {
        services.AddSingleton<THandler>();
        services.AddSingleton<IAsyncEventHandler<TEvent>>(sp => sp.GetRequiredService<THandler>());

        return services;
    }

    /// <summary>
    /// Registers <typeparamref name="THandler" /> as an <see cref="ITickEventHandler{TEvent}" /> singleton.
    /// </summary>
    public static IServiceCollection AddTickEventHandler<THandler, TEvent>(this IServiceCollection services)
        where THandler : class, ITickEventHandler<TEvent>
        where TEvent : ITickEvent
    {
        services.AddSingleton<THandler>();
        services.AddSingleton<ITickEventHandler<TEvent>>(sp => sp.GetRequiredService<THandler>());

        return services;
    }
}
