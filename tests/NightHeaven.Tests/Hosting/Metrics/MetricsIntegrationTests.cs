using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NightHeaven.Hosting.Interfaces.Metrics;
using NightHeaven.Server.Extensions;
using NightHeaven.Server.Services.Metrics;

namespace NightHeaven.Tests.Hosting.Metrics;

public class MetricsIntegrationTests
{
    [Fact]
    public async Task FullHost_AllProvidersAggregatedAndFormatted()
    {
        var services = new ServiceCollection();
        services.AddNightHeavenEventBus();
        services.AddNightHeavenTimerWheel();
        services.AddNightHeavenMetrics(cfg => cfg.RefreshInterval = TimeSpan.FromMilliseconds(50));

        services.AddMetricProvider<NightHeaven.Server.Services.EventBus.EventBusService>();
        services.AddMetricProvider<NightHeaven.Server.Services.GameLoop.GameLoopService>();
        services.AddMetricProvider<NightHeaven.Server.Services.Timing.TimerWheelService>();

        var sp = services.BuildServiceProvider();
        var orchestrator = sp.GetRequiredService<IEnumerable<IHostedService>>().Single();
        var metrics = sp.GetRequiredService<IMetricsService>();

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
