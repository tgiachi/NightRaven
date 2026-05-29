using NightHeaven.Hosting.Types.Metrics;

namespace NightHeaven.Hosting.Data.Metrics;

/// <summary>
/// One collected metric datapoint returned by an <see cref="Interfaces.Metrics.IMetricProvider" />.
/// </summary>
/// <param name="Name">Sample name without provider prefix. The collector prepends the prefix.</param>
/// <param name="Value">Numeric value at collection time.</param>
/// <param name="Type">Counter or Gauge. Default Gauge.</param>
/// <param name="Tags">Optional label set (<c>{key="value",...}</c> in OpenMetrics output).</param>
/// <param name="Help">Optional one-line description. Becomes <c># HELP</c> in OpenMetrics output.</param>
/// <param name="Timestamp">Optional point-in-time override. Defaults to collection time.</param>
public sealed record MetricSample(
    string Name,
    double Value,
    MetricType Type = MetricType.Gauge,
    IReadOnlyDictionary<string, string>? Tags = null,
    string? Help = null,
    DateTimeOffset? Timestamp = null
);
