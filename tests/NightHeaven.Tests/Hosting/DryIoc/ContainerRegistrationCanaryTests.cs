using global::DryIoc;
using NightHeaven.Hosting.Interfaces.Services;
using NightHeaven.Server.Extensions.DryIoc;
using NightHeaven.Tests.Support;

namespace NightHeaven.Tests.Hosting.DryIocNative;

public class ContainerRegistrationCanaryTests
{
    [Fact]
    public async Task EventBus_RegisteredNatively_ResolvesAndStartsViaOrchestrator()
    {
        var container = new Container();
        container.AddNightHeavenEventBus();

        // Interface aliases resolve to the same singleton instances.
        Assert.NotNull(container.Resolve<IEventBusService>());
        Assert.NotNull(container.Resolve<IGameLoopService>());

        // The orchestrator drives the lifecycle (surfaced as the host's IHostedService in Program.cs).
        var orchestrator = container.Orchestrator();
        Assert.Equal("NightHeavenServiceOrchestrator", orchestrator.GetType().Name);

        await orchestrator.StartAsync(CancellationToken.None);
        await orchestrator.StopAsync(CancellationToken.None);
    }

    [Fact]
    public void AddNightHeavenHosting_CalledTwice_ResolvesSingleOrchestrator()
    {
        var container = new Container();
        container.AddNightHeavenHosting();
        container.AddNightHeavenHosting();

        Assert.Same(container.Orchestrator(), container.Orchestrator());
    }
}
