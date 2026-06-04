using DryIoc;
using NightRaven.Abstractions.Data;
using NightRaven.Abstractions.Extensions.DryIoc;
using NightRaven.Abstractions.Interfaces.Services;
using NightRaven.Server.Extensions.Hosting;
using NightRaven.Server.Services.EventBus;
using NightRaven.Server.Services.GameLoop;

namespace NightRaven.Server.Extensions.EventBus;

/// <summary>
/// DryIoc-native bootstrap helpers for the NightRaven event bus + game loop.
/// </summary>
public static class EventBusContainerExtensions
{
    private const int EventBusPriority = 0;
    private const int GameLoopPriority = 10;

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
}
