namespace NightHeaven.Hosting.Data.Metrics;

/// <summary>
/// Point-in-time aggregation of every <see cref="Interfaces.Metrics.IMetricProvider" />'s samples.
/// Returned by <see cref="Interfaces.Metrics.IMetricsService.GetSnapshot" />.
/// </summary>
/// <param name="CollectedAt">Wall-clock time at which the providers were polled.</param>
/// <param name="Samples">Flat list of samples with provider prefix already applied to each name.</param>
public sealed record MetricsSnapshot(
    DateTimeOffset CollectedAt,
    IReadOnlyList<MetricSample> Samples
);
