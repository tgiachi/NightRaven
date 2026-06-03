using DryIoc;
using NightHeaven.Hosting.Data.Metrics;
using NightHeaven.Hosting.Interfaces.Metrics;
using NightHeaven.Server.Services.Metrics;

namespace NightHeaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for the NightHeaven metrics service.
/// </summary>
public static class MetricsContainerExtensions
{
    private const int MetricsServicePriority = 5;

    /// <summary>
    /// Registers an alias from <see cref="IMetricProvider" /> to the existing singleton
    /// <typeparamref name="TProvider" />. The provider itself must already be registered as a singleton.
    /// </summary>
    public static IContainer AddMetricProvider<TProvider>(this IContainer container)
        where TProvider : class, IMetricProvider
    {
        container.RegisterMapping<IMetricProvider, TProvider>();

        return container;
    }

    /// <summary>
    /// Registers <see cref="MetricsService" /> with the NightHeaven hosting orchestrator.
    /// Requires <c>AddNightHeavenTimerWheel</c> to have been called earlier so
    /// <see cref="Hosting.Interfaces.Timing.ITimerService" /> is resolvable.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    /// <param name="configure">Optional callback to customize <see cref="MetricsConfig" />.</param>
    public static IContainer AddNightHeavenMetrics(
        this IContainer container,
        Action<MetricsConfig>? configure = null
    )
    {
        container.AddNightHeavenHosting();

        var config = new MetricsConfig();
        configure?.Invoke(config);
        container.RegisterInstance(config);

        container.AddNightHeavenService<IMetricsService, MetricsService>(MetricsServicePriority);

        return container;
    }
}
