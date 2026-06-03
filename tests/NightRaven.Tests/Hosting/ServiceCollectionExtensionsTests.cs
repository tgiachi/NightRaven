using DryIoc;
using NightHeaven.Hosting.Interfaces.Services;
using NightHeaven.Server.Extensions.DryIoc;
using NightHeaven.Tests.Support;

namespace NightHeaven.Tests.Hosting;

public class ServiceCollectionExtensionsTests
{
    private interface IFooService : INightHeavenService;

    private sealed class FooService : IFooService
    {
        public Task StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    [Fact]
    public void AddNightHeavenHosting_CalledTwice_ResolvesSingleOrchestrator()
    {
        var container = new Container();
        container.AddNightHeavenHosting();
        container.AddNightHeavenHosting();

        // Idempotent: the orchestrator resolves as a single instance.
        Assert.NotNull(container.Orchestrator());
        Assert.Same(container.Orchestrator(), container.Orchestrator());
    }

    [Fact]
    public void AddNightHeavenService_WithInterface_RegistersSingletonAndAlias()
    {
        var container = new Container();
        container.AddNightHeavenService<IFooService, FooService>();

        var asInterface = container.Resolve<IFooService>();
        var asImpl = container.Resolve<FooService>();

        Assert.NotNull(asImpl);
        Assert.NotNull(asInterface);
        Assert.Same(asImpl, asInterface);
    }

    [Fact]
    public void AddNightHeavenService_WithoutInterface_RegistersImplementationOnly()
    {
        var container = new Container();
        container.AddNightHeavenService<FooService>();

        Assert.NotNull(container.Resolve<FooService>());
    }
}
