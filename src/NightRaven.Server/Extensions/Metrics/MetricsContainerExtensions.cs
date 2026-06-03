using DryIoc;
using NightRaven.Abstractions.Data.Metrics;
using NightRaven.Abstractions.Extensions.DryIoc;
using NightRaven.Abstractions.Interfaces.Metrics;
using NightRaven.Server.Extensions.Hosting;
using NightRaven.Server.Services.Metrics;

namespace NightRaven.Server.Extensions.Metrics;

/// <summary>
/// DryIoc-native bootstrap helpers for the NightRaven metrics service.
/// </summary>
public static class MetricsContainerExtensions
{
    private const int MetricsServicePriority = 5;

    /// <param name="container">DryIoc container.</param>
    extension(IContainer container)
    {
        /// <summary>
        /// Registers <see cref="MetricsService" /> with the NightRaven hosting orchestrator.
        /// Requires <c>AddNightRavenTimerWheel</c> to have been called earlier so
        /// <see cref="Hosting.Interfaces.Timing.ITimerService" /> is resolvable.
        /// </summary>
        public IContainer AddNightRavenMetrics()
        {
            container.AddNightRavenHosting();

            container.RegisterConfigSection("metrics", () => new MetricsConfig());

            container.AddNightRavenService<IMetricsService, MetricsService>(MetricsServicePriority);

            return container;
        }
    }
}
