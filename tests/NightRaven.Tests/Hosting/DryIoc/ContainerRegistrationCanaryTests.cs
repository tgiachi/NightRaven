using DryIoc;
using NightRaven.Abstractions.Interfaces.Services;
using NightRaven.Server.Extensions.DryIoc;
using NightRaven.Tests.Support;

namespace NightRaven.Tests.Hosting.DryIocNative;

public class ContainerRegistrationCanaryTests : IDisposable
{
    private readonly string _dir = Path.Combine(
        Path.GetTempPath(),
        $"nightraven-container-canary-{Guid.NewGuid():N}"
    );

    private string ConfigPath => Path.Combine(_dir, "nightraven.toml");

    [Fact]
    public void AddNightRavenHosting_CalledTwice_ResolvesSingleOrchestrator()
    {
        var container = new Container();
        container.AddNightRavenHosting();
        container.AddNightRavenHosting();

        Assert.Same(container.Orchestrator(), container.Orchestrator());
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task EventBus_RegisteredNatively_ResolvesAndStartsViaOrchestrator()
    {
        var container = new Container();
        container.AddNightRavenEventBus();
        container.AddNightRavenConfig(ConfigPath);

        // Interface aliases resolve to the same singleton instances.
        Assert.NotNull(container.Resolve<IEventBusService>());
        Assert.NotNull(container.Resolve<IGameLoopService>());

        // The orchestrator drives the lifecycle (surfaced as the host's IHostedService in Program.cs).
        var orchestrator = container.Orchestrator();
        Assert.Equal("NightRavenServiceOrchestrator", orchestrator.GetType().Name);

        await orchestrator.StartAsync(CancellationToken.None);
        await orchestrator.StopAsync(CancellationToken.None);
    }
}
