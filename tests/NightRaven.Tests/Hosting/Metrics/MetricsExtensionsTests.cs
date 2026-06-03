using DryIoc;
using NightRaven.Abstractions.Data.Metrics;
using NightRaven.Abstractions.Extensions.DryIoc;
using NightRaven.Abstractions.Interfaces.Metrics;
using NightRaven.Server.Extensions.DryIoc;

namespace NightRaven.Tests.Hosting.Metrics;

public class MetricsExtensionsTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-metrics-config-{Guid.NewGuid():N}");
    private string Path_ => Path.Combine(_dir, "nightraven.toml");

    private sealed class NamedProvider : IMetricProvider
    {
        public string Prefix => "named";

        public IReadOnlyList<MetricSample> Collect()
            => [new("v", 1)];
    }

    [Fact]
    public void AddMetricProvider_AliasesAnExistingSingletonAsIMetricProvider()
    {
        var container = new Container();
        container.Register<NamedProvider>(Reuse.Singleton);
        container.AddMetricProvider<NamedProvider>();

        var providers = container.Resolve<IEnumerable<IMetricProvider>>().ToArray();

        Assert.Single(providers);
        Assert.IsType<NamedProvider>(providers[0]);
        Assert.Same(container.Resolve<NamedProvider>(), providers[0]);
    }

    [Fact]
    public void AddNightRavenMetrics_AppliesCustomConfig()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path_, "[metrics]\nrefresh_interval = \"00:00:02\"\n");

        var container = new Container();
        container.AddNightRavenTimerWheel();
        container.AddNightRavenMetrics();
        container.AddNightRavenConfig(Path_);

        var cfg = container.Resolve<MetricsConfig>();
        Assert.Equal(TimeSpan.FromSeconds(2), cfg.RefreshInterval);
    }

    [Fact]
    public void AddNightRavenMetrics_DefaultConfig_FiveSeconds()
    {
        var container = new Container();
        container.AddNightRavenTimerWheel();
        container.AddNightRavenMetrics();
        container.AddNightRavenConfig(Path_);

        var cfg = container.Resolve<MetricsConfig>();
        Assert.Equal(TimeSpan.FromSeconds(5), cfg.RefreshInterval);
    }

    [Fact]
    public void AddNightRavenMetrics_RegistersServiceAndConfig()
    {
        var container = new Container();
        container.AddNightRavenTimerWheel();
        container.AddNightRavenMetrics();
        container.AddNightRavenConfig(Path_);

        Assert.NotNull(container.Resolve<IMetricsService>());
        Assert.NotNull(container.Resolve<MetricsConfig>());
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }
}
