using NightHeaven.Hosting.Data.Metrics;
using NightHeaven.Hosting.Interfaces.Services;

namespace NightHeaven.Hosting.Interfaces.Metrics;

/// <summary>
/// Central service that aggregates samples from every registered <see cref="IMetricProvider" />
/// and exposes the latest snapshot for scraping.
/// </summary>
public interface IMetricsService : INightHeavenService
{
    /// <summary>
    /// Returns the most recently built snapshot. O(1) read backed by <see cref="Volatile" />.
    /// </summary>
    MetricsSnapshot GetSnapshot();
}
