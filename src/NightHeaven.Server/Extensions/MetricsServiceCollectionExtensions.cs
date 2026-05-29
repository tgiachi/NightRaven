using Microsoft.Extensions.DependencyInjection;
using NightHeaven.Hosting.Data.Metrics;
using NightHeaven.Hosting.Extensions;
using NightHeaven.Hosting.Interfaces.Metrics;
using NightHeaven.Server.Services.Metrics;

namespace NightHeaven.Server.Extensions;

/// <summary>
/// DI registration helpers for the NightHeaven metrics service.
/// </summary>
public static class MetricsServiceCollectionExtensions
{
    private const int MetricsServicePriority = 5;

    /// <summary>
    /// Registers <see cref="MetricsService" /> with the NightHeaven hosting orchestrator.
    /// Calls <see cref="ServiceCollectionExtensions.AddNightHeavenHosting" /> internally (idempotent).
    /// Requires <c>AddNightHeavenTimerWheel</c> to have been called earlier so <see cref="Hosting.Interfaces.Timing.ITimerService" /> is resolvable.
    /// </summary>
    /// <param name="services">DI service collection.</param>
    /// <param name="configure">Optional callback to customize <see cref="MetricsConfig" />.</param>
    public static IServiceCollection AddNightHeavenMetrics(
        this IServiceCollection services,
        Action<MetricsConfig>? configure = null
    )
    {
        services.AddNightHeavenHosting();

        var config = new MetricsConfig();
        configure?.Invoke(config);
        services.AddSingleton(config);

        services.AddNightHeavenService<IMetricsService, MetricsService>(MetricsServicePriority);

        return services;
    }

    /// <summary>
    /// Registers an alias from <see cref="IMetricProvider" /> to the existing singleton <typeparamref name="TProvider" />.
    /// The provider itself must already be registered as a singleton.
    /// </summary>
    public static IServiceCollection AddMetricProvider<TProvider>(this IServiceCollection services)
        where TProvider : class, IMetricProvider
    {
        services.AddSingleton<IMetricProvider>(sp => sp.GetRequiredService<TProvider>());

        return services;
    }
}
