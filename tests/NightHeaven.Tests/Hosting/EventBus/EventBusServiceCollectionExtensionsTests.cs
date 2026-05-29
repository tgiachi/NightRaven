using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NightHeaven.Hosting.Data;
using NightHeaven.Hosting.Interfaces;
using NightHeaven.Server.Extensions;
using NightHeaven.Tests.Hosting.EventBus.Support;

namespace NightHeaven.Tests.Hosting.EventBus;

public class EventBusServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNightHeavenEventBus_RegistersBusAndGameLoopAsHostedServices()
    {
        var services = new ServiceCollection();

        services.AddNightHeavenEventBus();

        var sp = services.BuildServiceProvider();
        var hosted = sp.GetServices<IHostedService>().ToArray();

        // Orchestrator from AddNightHeavenHosting is the single hosted service.
        Assert.Single(hosted);

        // Bus and game loop are resolvable through their interfaces.
        Assert.NotNull(sp.GetService<IEventBusService>());
        Assert.NotNull(sp.GetService<IGameLoopService>());
    }

    [Fact]
    public void AddNightHeavenEventBus_CustomConfig_AppliesConfig()
    {
        var services = new ServiceCollection();

        services.AddNightHeavenEventBus(cfg => { cfg.IdleSleepMs = 7; cfg.IdleCpuEnabled = false; });

        var sp = services.BuildServiceProvider();
        var cfg = sp.GetRequiredService<GameLoopConfig>();

        Assert.Equal(7, cfg.IdleSleepMs);
        Assert.False(cfg.IdleCpuEnabled);
    }

    [Fact]
    public void AddNightHeavenEventBus_NoCustomConfig_AppliesDefaults()
    {
        var services = new ServiceCollection();

        services.AddNightHeavenEventBus();

        var cfg = services.BuildServiceProvider().GetRequiredService<GameLoopConfig>();

        Assert.True(cfg.IdleCpuEnabled);
        Assert.Equal(1, cfg.IdleSleepMs);
    }

    [Fact]
    public async Task AddAsyncEventHandler_RegistersHandlerAndItIsInvokedByBus()
    {
        var timeline = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(timeline);
        services.AddNightHeavenEventBus();
        services.AddAsyncEventHandler<NamedAsyncHandler, TestAsyncEvent>();

        var sp = services.BuildServiceProvider();
        await sp.GetRequiredService<IEventBusService>().PublishAsync(new TestAsyncEvent("hello"));

        Assert.Equal(new[] { "async:Named:hello" }, timeline);
    }

    [Fact]
    public void AddTickEventHandler_RegistersHandlerAndDrainInvokesIt()
    {
        var timeline = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(timeline);
        services.AddNightHeavenEventBus();
        services.AddTickEventHandler<NamedTickHandler, TestTickEvent>();

        var sp = services.BuildServiceProvider();
        var bus = sp.GetRequiredService<IEventBusService>();
        bus.Publish(new TestTickEvent(11));

        var processed = bus.DrainTickEvents(maxItems: 10);

        Assert.Equal(1, processed);
        Assert.Equal(new[] { "tick:Named:11" }, timeline);
    }

    private sealed class NamedAsyncHandler : IAsyncEventHandler<TestAsyncEvent>
    {
        private readonly List<string> _timeline;

        public NamedAsyncHandler(List<string> timeline)
        {
            _timeline = timeline;
        }

        public Task HandleAsync(TestAsyncEvent evt, CancellationToken cancellationToken)
        {
            lock (_timeline)
            {
                _timeline.Add($"async:Named:{evt.Payload}");
            }

            return Task.CompletedTask;
        }
    }

    private sealed class NamedTickHandler : ITickEventHandler<TestTickEvent>
    {
        private readonly List<string> _timeline;

        public NamedTickHandler(List<string> timeline)
        {
            _timeline = timeline;
        }

        public void Handle(TestTickEvent evt)
        {
            lock (_timeline)
            {
                _timeline.Add($"tick:Named:{evt.Value}");
            }
        }
    }
}
