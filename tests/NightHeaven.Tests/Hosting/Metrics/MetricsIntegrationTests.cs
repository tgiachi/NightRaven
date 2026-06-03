using global::DryIoc;
using NightHeaven.Hosting.Interfaces.Metrics;
using NightHeaven.Server.Extensions.DryIoc;
using NightHeaven.Server.Services.EventBus;
using NightHeaven.Server.Services.GameLoop;
using NightHeaven.Server.Services.Metrics;
using NightHeaven.Server.Services.Timing;
using NightHeaven.Tests.Support;

namespace NightHeaven.Tests.Hosting.Metrics;

public class MetricsIntegrationTests
{
    [Fact]
    public async Task FullHost_AllProvidersAggregatedAndFormatted()
    {
        var container = new Container();
        container.AddNightHeavenEventBus();
        container.AddNightHeavenTimerWheel();
        container.AddNightHeavenMetrics(cfg => cfg.RefreshInterval = TimeSpan.FromMilliseconds(50));

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
