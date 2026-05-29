using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NightHeaven.Hosting.Data.Metrics;
using NightHeaven.Hosting.Interfaces.Metrics;
using NightHeaven.Server.Extensions;

namespace NightHeaven.Tests.Hosting.Metrics;

public class MetricsExtensionsTests
{
    private sealed class NamedProvider : IMetricProvider
    {
        public string Prefix => "named";

        public IReadOnlyList<MetricSample> Collect()
            => [new("v", 1)];
    }

    [Fact]
    public void AddMetricProvider_AliasesAnExistingSingletonAsIMetricProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<NamedProvider>();
        services.AddMetricProvider<NamedProvider>();

        var sp = services.BuildServiceProvider();
        var providers = sp.GetServices<IMetricProvider>().ToArray();

        Assert.Single(providers);
        Assert.IsType<NamedProvider>(providers[0]);
        Assert.Same(sp.GetRequiredService<NamedProvider>(), providers[0]);
    }

    [Fact]
    public void AddNightHeavenMetrics_AppliesCustomConfig()
    {
        var services = new ServiceCollection();
        services.AddNightHeavenTimerWheel();
        services.AddNightHeavenMetrics(cfg => cfg.RefreshInterval = TimeSpan.FromSeconds(2));

        var cfg = services.BuildServiceProvider().GetRequiredService<MetricsConfig>();
        Assert.Equal(TimeSpan.FromSeconds(2), cfg.RefreshInterval);
    }

    [Fact]
    public void AddNightHeavenMetrics_DefaultConfig_FiveSeconds()
    {
        var services = new ServiceCollection();
        services.AddNightHeavenTimerWheel();
        services.AddNightHeavenMetrics();

        var cfg = services.BuildServiceProvider().GetRequiredService<MetricsConfig>();
        Assert.Equal(TimeSpan.FromSeconds(5), cfg.RefreshInterval);
    }

    [Fact]
    public void AddNightHeavenMetrics_OrchestratorRegisteredOnce()
    {
        var services = new ServiceCollection();
        services.AddNightHeavenTimerWheel();
        services.AddNightHeavenMetrics();

        var sp = services.BuildServiceProvider();
        var hosted = sp.GetServices<IHostedService>().ToArray();

        Assert.Single(hosted);
        Assert.Equal("NightHeavenServiceOrchestrator", hosted[0].GetType().Name);
    }

    [Fact]
    public void AddNightHeavenMetrics_RegistersServiceAndConfig()
    {
        var services = new ServiceCollection();
        services.AddNightHeavenTimerWheel();
        services.AddNightHeavenMetrics();

        var sp = services.BuildServiceProvider();

        Assert.NotNull(sp.GetService<IMetricsService>());
        Assert.NotNull(sp.GetService<MetricsConfig>());
    }
}
