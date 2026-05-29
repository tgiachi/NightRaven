using NightHeaven.Hosting.Data.Metrics;
using NightHeaven.Hosting.Interfaces.Metrics;
using NightHeaven.Hosting.Interfaces.Timing;
using Serilog;
using ILogger = Serilog.ILogger;

namespace NightHeaven.Server.Services.Metrics;

/// <summary>
/// Background metrics aggregator. Builds a <see cref="MetricsSnapshot" /> every
/// <see cref="MetricsConfig.RefreshInterval" /> from a timer registered on
/// <see cref="ITimerService" />, and serves the cached snapshot on every scrape.
/// </summary>
public sealed class MetricsService : IMetricsService
{
    private const string RefreshTimerName = "metrics-refresh";

    private readonly ILogger _logger = Log.ForContext<MetricsService>();
    private readonly IReadOnlyList<IMetricProvider> _providers;
    private readonly ITimerService _timer;
    private readonly MetricsConfig _config;

    private MetricsSnapshot _latestSnapshot;
    private string? _timerId;

    public MetricsService(IEnumerable<IMetricProvider> providers, ITimerService timer, MetricsConfig config)
    {
        _providers = providers.ToArray();
        _timer = timer;
        _config = config;
        _latestSnapshot = new MetricsSnapshot(DateTimeOffset.MinValue, Array.Empty<MetricSample>());
    }

    public MetricsSnapshot GetSnapshot() => Volatile.Read(ref _latestSnapshot);

    public Task StartAsync(CancellationToken cancellationToken)
    {
        RefreshSnapshot();
        _timerId = _timer.RegisterTimer(
            name: RefreshTimerName,
            interval: _config.RefreshInterval,
            callback: RefreshSnapshot,
            repeat: true
        );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_timerId is not null)
        {
            _timer.UnregisterTimer(_timerId);
            _timerId = null;
        }

        return Task.CompletedTask;
    }

    private void RefreshSnapshot()
    {
        var collectedAt = DateTimeOffset.UtcNow;
        var samples = new List<MetricSample>(_providers.Count * 4);

        for (var i = 0; i < _providers.Count; i++)
        {
            var provider = _providers[i];

            try
            {
                var providerSamples = provider.Collect();

                for (var s = 0; s < providerSamples.Count; s++)
                {
                    var sample = providerSamples[s];
                    samples.Add(sample with { Name = $"{provider.Prefix}_{sample.Name}" });
                }
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "MetricProvider {Provider} failed to Collect; skipped",
                    provider.GetType().Name
                );
            }
        }

        Volatile.Write(ref _latestSnapshot, new MetricsSnapshot(collectedAt, samples));
    }
}
