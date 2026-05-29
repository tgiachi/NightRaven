using Microsoft.Extensions.DependencyInjection;
using NightHeaven.Hosting.Data.Timing;
using NightHeaven.Hosting.Extensions;
using NightHeaven.Hosting.Interfaces.Timing;
using NightHeaven.Server.Services.Timing;

namespace NightHeaven.Server.Extensions;

/// <summary>
/// DI registration helpers for the NightHeaven timer wheel.
/// </summary>
public static class TimerServiceCollectionExtensions
{
    private const int TimerWheelPriority = 3;

    /// <summary>
    /// Registers <see cref="TimerWheelService" /> with the NightHeaven hosting orchestrator.
    /// Calls <see cref="ServiceCollectionExtensions.AddNightHeavenHosting" /> internally (idempotent).
    /// </summary>
    /// <param name="services">DI service collection.</param>
    /// <param name="configure">Optional callback to customize <see cref="TimerWheelConfig" />.</param>
    public static IServiceCollection AddNightHeavenTimerWheel(
        this IServiceCollection services,
        Action<TimerWheelConfig>? configure = null
    )
    {
        services.AddNightHeavenHosting();

        var config = new TimerWheelConfig();
        configure?.Invoke(config);
        services.AddSingleton(config);

        services.AddNightHeavenService<ITimerService, TimerWheelService>(TimerWheelPriority);

        return services;
    }
}
