using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NightHeaven.Hosting.Extensions;
using NightHeaven.Hosting.Interfaces;

namespace NightHeaven.Tests.Hosting;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNightHeavenService_WithInterface_RegistersSingletonAndAlias()
    {
        var services = new ServiceCollection();
        var timeline = new List<string>();
        services.AddSingleton(timeline);
        services.AddSingleton<IFooService>(_ => new FooService(timeline));
        // Register the implementation via the full overload to also alias the interface.
        services.AddNightHeavenService<IFooService, FooService>();

        var sp = services.BuildServiceProvider();

        var asInterface = sp.GetService<IFooService>();
        var asImpl = sp.GetService<FooService>();

        Assert.NotNull(asImpl);
        Assert.NotNull(asInterface);
        Assert.Same(asImpl, asInterface);
    }

    [Fact]
    public void AddNightHeavenService_WithoutInterface_RegistersImplementationOnly()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new List<string>());
        services.AddNightHeavenService<FooService>();

        var sp = services.BuildServiceProvider();

        Assert.NotNull(sp.GetService<FooService>());
    }

    [Fact]
    public void AddNightHeavenHosting_RegistersOrchestratorAsHostedService()
    {
        var services = new ServiceCollection();
        services.AddNightHeavenHosting();

        var sp = services.BuildServiceProvider();

        var hostedServices = sp.GetServices<IHostedService>().ToArray();

        Assert.Single(hostedServices);
        Assert.Equal("NightHeavenServiceOrchestrator", hostedServices[0].GetType().Name);
    }

    [Fact]
    public void AddNightHeavenHosting_CalledTwice_RegistersOrchestratorOnce()
    {
        var services = new ServiceCollection();
        services.AddNightHeavenHosting();
        services.AddNightHeavenHosting();

        var sp = services.BuildServiceProvider();

        Assert.Single(sp.GetServices<IHostedService>());
    }

    private interface IFooService : INightHeavenService;

    private sealed class FooService : IFooService
    {
        private readonly List<string> _timeline;

        public FooService(List<string> timeline)
        {
            _timeline = timeline;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timeline.Add("start:foo");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timeline.Add("stop:foo");

            return Task.CompletedTask;
        }
    }
}
