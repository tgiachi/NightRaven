using global::DryIoc;
using NightHeaven.Server.Extensions.DryIoc;
using NightHeaven.Tests.Hosting.Configuration.Support;

namespace NightHeaven.Tests.Hosting.Configuration;

public class ConfigContainerExtensionsTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-config-di-{Guid.NewGuid():N}");
    private string Path_ => Path.Combine(_dir, "nightheaven.toml");

    [Fact]
    public void AddNightHeavenConfig_RegistersBoundInstance()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(Path_, "[server]\nport = 9000\n");

        var container = new Container();
        container.RegisterConfigSection<TestServerSettings>("server", () => new TestServerSettings());
        container.AddNightHeavenConfig(Path_);

        Assert.Equal(9000, container.Resolve<TestServerSettings>().Port);
    }

    [Fact]
    public void AddNightHeavenConfig_NoSections_CreatesNothingAndDoesNotThrow()
    {
        var container = new Container();

        container.AddNightHeavenConfig(Path_);

        Assert.False(File.Exists(Path_));
    }

    [Fact]
    public void AddNightHeavenConfig_MissingFile_CreatesDefaultAndRegistersDefault()
    {
        var container = new Container();
        container.RegisterConfigSection<TestServerSettings>("server", () => new TestServerSettings());

        container.AddNightHeavenConfig(Path_);

        Assert.True(File.Exists(Path_));
        Assert.Equal(2593, container.Resolve<TestServerSettings>().Port);
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
