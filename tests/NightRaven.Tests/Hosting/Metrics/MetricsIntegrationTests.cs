using DryIoc;
using NightRaven.Abstractions.Extensions.DryIoc;
using NightRaven.Abstractions.Interfaces.Metrics;
using NightRaven.Server.Extensions.Configuration;
using NightRaven.Server.Extensions.EventBus;
using NightRaven.Server.Extensions.Metrics;
using NightRaven.Server.Extensions.Timing;
using NightRaven.Server.Services.EventBus;
using NightRaven.Server.Services.GameLoop;
using NightRaven.Server.Services.Metrics;
using NightRaven.Server.Services.Timing;
using NightRaven.Tests.Support;

namespace NightRaven.Tests.Hosting.Metrics;

public class MetricsIntegrationTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-metrics-integration-config-{Guid.NewGuid():N}");
    private string Path_ => Path.Combine(_dir, "nightraven.toml");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task FullHost_AllProvidersAggregatedAndFormatted()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path_, "[metrics]\nrefresh_interval = \"00:00:00.0500000\"\n");

        var container = new Container();
        container.AddNightRavenEventBus();
        container.AddNightRavenTimerWheel();
        container.AddNightRavenMetrics();
        container.AddNightRavenConfig(Path_);

        container.AddMetricProvider<EventBusService>();
        container.AddMetricProvider<GameLoopService>();
        container.AddMetricProvider<TimerWheelService>();

        var orchestrator = container.Orchestrator();
        var metrics = container.Resolve<IMetricsService>();

        await orchestrator.StartAsync(CancellationToken.None);

        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);

        while (DateTime.UtcNow < deadline && metrics.GetSnapshot().Samples.Count == 0)
        {
            await Task.Delay(20);
        }

        var snapshot = metrics.GetSnapshot();
        var names = snapshot.Samples.Select(s => s.Name).ToHashSet();

        Assert.Contains("bus_tick_queue_depth", names);
        Assert.Contains("gameloop_tick_count", names);
        Assert.Contains("timer_active", names);

        var text = OpenMetricsFormatter.Format(snapshot);
        Assert.Contains("# TYPE bus_tick_queue_depth gauge", text);
        Assert.Contains("# TYPE gameloop_tick_count_total counter", text);
        Assert.EndsWith("# EOF\n", text);

        await orchestrator.StopAsync(CancellationToken.None);
    }
}
