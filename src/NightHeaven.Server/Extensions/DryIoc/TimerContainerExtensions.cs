using DryIoc;
using NightHeaven.Hosting.Data.Timing;
using NightHeaven.Hosting.Interfaces.Timing;
using NightHeaven.Server.Services.Timing;

namespace NightHeaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for the NightHeaven timer wheel.
/// </summary>
public static class TimerContainerExtensions
{
    private const int TimerWheelPriority = 3;

    /// <summary>
    /// Registers <see cref="TimerWheelService" /> with the NightHeaven hosting orchestrator.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    public static IContainer AddNightHeavenTimerWheel(this IContainer container)
    {
        container.AddNightHeavenHosting();

        container.RegisterConfigSection("timing", () => new TimerWheelConfig());

        container.AddNightHeavenService<ITimerService, TimerWheelService>(TimerWheelPriority);

        return container;
    }
}
