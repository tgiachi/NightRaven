using DryIoc;
using NightRaven.Server.Extensions.DryIoc;
using NightRaven.Tests.Hosting.Configuration.Support;

namespace NightRaven.Tests.Hosting.Configuration;

public class ConfigContainerExtensionsTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-config-di-{Guid.NewGuid():N}");
    private string Path_ => Path.Combine(_dir, "nightraven.toml");

    [Fact]
    public void AddNightRavenConfig_MissingFile_CreatesDefaultAndRegistersDefault()
    {
        var container = new Container();
        container.RegisterConfigSection("server", () => new TestServerSettings());

        container.AddNightRavenConfig(Path_);

        Assert.True(File.Exists(Path_));
        Assert.Equal(2593, container.Resolve<TestServerSettings>().Port);
    }

    [Fact]
    public void AddNightRavenConfig_NoSections_CreatesNothingAndDoesNotThrow()
    {
        var container = new Container();

        container.AddNightRavenConfig(Path_);

        Assert.False(File.Exists(Path_));
    }

    [Fact]
    public void AddNightRavenConfig_RegistersBoundInstance()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path_, "[server]\nport = 9000\n");

        var container = new Container();
        container.RegisterConfigSection("server", () => new TestServerSettings());
        container.AddNightRavenConfig(Path_);

        Assert.Equal(9000, container.Resolve<TestServerSettings>().Port);
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
