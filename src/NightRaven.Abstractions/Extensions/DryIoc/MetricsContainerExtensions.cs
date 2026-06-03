using DryIoc;
using NightRaven.Abstractions.Interfaces.Metrics;

namespace NightRaven.Abstractions.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for NightRaven metric providers.
/// </summary>
public static class MetricsContainerExtensions
{
    extension(IContainer container)
    {
        /// <summary>
        /// Registers an alias from <see cref="IMetricProvider" /> to the existing singleton
        /// <typeparamref name="TProvider" />. The provider itself must already be registered as a singleton.
        /// </summary>
        public IContainer AddMetricProvider<TProvider>()
            where TProvider : class, IMetricProvider
        {
            container.RegisterMapping<IMetricProvider, TProvider>();

            return container;
        }
    }
}
