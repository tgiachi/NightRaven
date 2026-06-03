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
    /// <param name="configure">Optional callback to customize <see cref="TimerWheelConfig" />.</param>
    public static IContainer AddNightHeavenTimerWheel(
        this IContainer container,
        Action<TimerWheelConfig>? configure = null
    )
    {
        container.AddNightHeavenHosting();

        var config = new TimerWheelConfig();
        configure?.Invoke(config);
        container.RegisterInstance(config);

        container.AddNightHeavenService<ITimerService, TimerWheelService>(TimerWheelPriority);

        return container;
    }
}
