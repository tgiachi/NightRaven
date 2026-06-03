namespace NightRaven.Hosting.Data.Metrics;

/// <summary>
/// Configuration for <see cref="Interfaces.Metrics.IMetricsService" />.
/// </summary>
public sealed class MetricsConfig
{
    /// <summary>
    /// How often the background refresh polls every <see cref="Interfaces.Metrics.IMetricProvider" />.
    /// Default 5 s.
    /// </summary>
    public TimeSpan RefreshInterval { get; set; } = TimeSpan.FromSeconds(5);
}
