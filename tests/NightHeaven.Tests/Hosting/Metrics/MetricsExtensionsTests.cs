using DryIoc;
using NightHeaven.Hosting.Data.Metrics;
using NightHeaven.Hosting.Interfaces.Metrics;
using NightHeaven.Server.Extensions.DryIoc;

namespace NightHeaven.Tests.Hosting.Metrics;

public class MetricsExtensionsTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-metrics-config-{Guid.NewGuid():N}");
    private string Path_ => Path.Combine(_dir, "nightheaven.toml");

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
    public void AddNightHeavenMetrics_AppliesCustomConfig()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path_, "[metrics]\nrefresh_interval = \"00:00:02\"\n");

        var container = new Container();
        container.AddNightHeavenTimerWheel();
        container.AddNightHeavenMetrics();
        container.AddNightHeavenConfig(Path_);

        var cfg = container.Resolve<MetricsConfig>();
        Assert.Equal(TimeSpan.FromSeconds(2), cfg.RefreshInterval);
    }

    [Fact]
    public void AddNightHeavenMetrics_DefaultConfig_FiveSeconds()
    {
        var container = new Container();
        container.AddNightHeavenTimerWheel();
        container.AddNightHeavenMetrics();
        container.AddNightHeavenConfig(Path_);

        var cfg = container.Resolve<MetricsConfig>();
        Assert.Equal(TimeSpan.FromSeconds(5), cfg.RefreshInterval);
    }

    [Fact]
    public void AddNightHeavenMetrics_RegistersServiceAndConfig()
    {
        var container = new Container();
        container.AddNightHeavenTimerWheel();
        container.AddNightHeavenMetrics();
        container.AddNightHeavenConfig(Path_);

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
