using DryIoc;
using NightRaven.Hosting.Data.Metrics;
using NightRaven.Hosting.Interfaces.Metrics;
using NightRaven.Server.Services.Metrics;

namespace NightRaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for the NightRaven metrics service.
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
    /// Registers <see cref="MetricsService" /> with the NightRaven hosting orchestrator.
    /// Requires <c>AddNightRavenTimerWheel</c> to have been called earlier so
    /// <see cref="Hosting.Interfaces.Timing.ITimerService" /> is resolvable.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    public static IContainer AddNightRavenMetrics(this IContainer container)
    {
        container.AddNightRavenHosting();

        container.RegisterConfigSection("metrics", () => new MetricsConfig());

        container.AddNightRavenService<IMetricsService, MetricsService>(MetricsServicePriority);

        return container;
    }
}
