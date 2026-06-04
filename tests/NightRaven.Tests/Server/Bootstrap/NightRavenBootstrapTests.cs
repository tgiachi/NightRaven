using DryIoc;
using NightRaven.Abstractions.Data.Logging;
using NightRaven.Abstractions.Interfaces.Metrics;
using NightRaven.Abstractions.Interfaces.Services;
using NightRaven.Abstractions.Interfaces.Timing;
using NightRaven.Core.Types;
using NightRaven.Network.UO.Registry;
using NightRaven.Scripting.Lua.Interfaces.Scripts;
using NightRaven.Server.Bootstrap;

namespace NightRaven.Tests.Server.Bootstrap;

public sealed class NightRavenBootstrapTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"nr-bootstrap-{Guid.NewGuid():N}");

    [Fact]
    public void ConfigureContainer_RegistersCoreServicesAndConfig()
    {
        using var container = new Container();
        var context = NightRavenBootstrap.CreateContext(new([], _root, false, false));

        NightRavenBootstrap.ConfigureContainer(container, context);

        Assert.Same(context.PacketRegistry, container.Resolve<PacketRegistry>());
        Assert.NotNull(container.Resolve<IEventBusService>());
        Assert.NotNull(container.Resolve<ITimerService>());
        Assert.NotNull(container.Resolve<IMetricsService>());
        Assert.NotNull(container.Resolve<IScriptEngineService>());
        Assert.NotNull(container.Resolve<LoggerConfig>());
        Assert.True(File.Exists(Path.Combine(context.Directories[DirectoryType.Config], "nightraven.toml")));
    }

    [Fact]
    public void CreateContext_ResolvesDirectoriesAndRegistersPackets()
    {
        var options = new NightRavenBootstrapOptions([], _root, false, false);

        var context = NightRavenBootstrap.CreateContext(options);

        Assert.Equal(_root, context.Directories.Root);
        Assert.True(context.RegisteredPacketCount > 0);
        Assert.NotNull(context.PacketRegistry);
        Assert.True(Directory.Exists(context.Directories[DirectoryType.Config]));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Options_ExposeCliValues()
    {
        var args = new[] { "--debug" };
        var options = new NightRavenBootstrapOptions(args, _root, true, false);

        Assert.Same(args, options.Args);
        Assert.True(options.Debug);
        Assert.False(options.ShowHeader);
        Assert.Equal(_root, options.RootDirectory);
    }
}
