using DryIoc;
using NightRaven.Hosting.Interfaces.Services;
using NightRaven.Server.Extensions.DryIoc;
using NightRaven.Tests.Support;

namespace NightRaven.Tests.Hosting;

public class ServiceCollectionExtensionsTests
{
    private interface IFooService : INightRavenService;

    private sealed class FooService : IFooService
    {
        public Task StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    [Fact]
    public void AddNightRavenHosting_CalledTwice_ResolvesSingleOrchestrator()
    {
        var container = new Container();
        container.AddNightRavenHosting();
        container.AddNightRavenHosting();

        // Idempotent: the orchestrator resolves as a single instance.
        Assert.NotNull(container.Orchestrator());
        Assert.Same(container.Orchestrator(), container.Orchestrator());
    }

    [Fact]
    public void AddNightRavenService_WithInterface_RegistersSingletonAndAlias()
    {
        var container = new Container();
        container.AddNightRavenService<IFooService, FooService>();

        var asInterface = container.Resolve<IFooService>();
        var asImpl = container.Resolve<FooService>();

        Assert.NotNull(asImpl);
        Assert.NotNull(asInterface);
        Assert.Same(asImpl, asInterface);
    }

    [Fact]
    public void AddNightRavenService_WithoutInterface_RegistersImplementationOnly()
    {
        var container = new Container();
        container.AddNightRavenService<FooService>();

        Assert.NotNull(container.Resolve<FooService>());
    }
}
