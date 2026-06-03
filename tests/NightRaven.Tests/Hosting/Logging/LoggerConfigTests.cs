using DryIoc;
using NightRaven.Core.Types;
using NightRaven.Hosting.Data.Logging;
using NightRaven.Server.Extensions.DryIoc;

namespace NightRaven.Tests.Hosting.Logging;

public sealed class LoggerConfigTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nr-logger-config-{Guid.NewGuid():N}");
    private string ConfigPath => Path.Combine(_dir, "nightraven.toml");

    [Fact]
    public void Defaults_MatchServerStartupLogging()
    {
        var config = new LoggerConfig();

        Assert.Equal(LogLevelType.Information, config.Level);
        Assert.False(config.LogPackets);
        Assert.False(config.WriteToFile);
        Assert.Equal("nightraven.log", config.FileName);
    }

    [Fact]
    public void AddNightRavenLogging_RegistersLoggerConfigSection()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(
            ConfigPath,
            "[logger]\nlevel = \"Debug\"\nlog_packets = true\nwrite_to_file = true\nfile_name = \"server.log\"\n"
        );

        var container = new Container();
        container.AddNightRavenLogging();
        container.AddNightRavenConfig(ConfigPath);

        var config = container.Resolve<LoggerConfig>();
        Assert.Equal(LogLevelType.Debug, config.Level);
        Assert.True(config.LogPackets);
        Assert.True(config.WriteToFile);
        Assert.Equal("server.log", config.FileName);
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
