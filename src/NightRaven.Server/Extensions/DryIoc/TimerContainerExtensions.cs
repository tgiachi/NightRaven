using DryIoc;
using NightRaven.Abstractions.Data.Timing;
using NightRaven.Abstractions.Extensions.DryIoc;
using NightRaven.Abstractions.Interfaces.Timing;
using NightRaven.Server.Services.Timing;

namespace NightRaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for the NightRaven timer wheel.
/// </summary>
public static class TimerContainerExtensions
{
    private const int TimerWheelPriority = 3;

    /// <summary>
    /// Registers <see cref="TimerWheelService" /> with the NightRaven hosting orchestrator.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    public static IContainer AddNightRavenTimerWheel(this IContainer container)
    {
        container.AddNightRavenHosting();

        container.RegisterConfigSection("timing", () => new TimerWheelConfig());

        container.AddNightRavenService<ITimerService, TimerWheelService>(TimerWheelPriority);

        return container;
    }
}
