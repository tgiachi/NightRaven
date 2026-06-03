using global::DryIoc;
using NightHeaven.Hosting.Data;
using NightHeaven.Hosting.Interfaces.EventHandlers;
using NightHeaven.Hosting.Interfaces.Services;
using NightHeaven.Server.Extensions.DryIoc;
using NightHeaven.Tests.Hosting.EventBus.Support;

namespace NightHeaven.Tests.Hosting.EventBus;

public class EventBusServiceCollectionExtensionsTests
{
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

    [Fact]
    public async Task AddAsyncEventHandler_RegistersHandlerAndItIsInvokedByBus()
    {
        var timeline = new List<string>();
        var container = new Container();
        container.RegisterInstance(timeline);
        container.AddNightHeavenEventBus();
        container.AddAsyncEventHandler<NamedAsyncHandler, TestAsyncEvent>();

        await container.Resolve<IEventBusService>().PublishAsync(new TestAsyncEvent("hello"));

        Assert.Equal(new[] { "async:Named:hello" }, timeline);
    }

    [Fact]
    public void AddNightHeavenEventBus_CustomConfig_AppliesConfig()
    {
        var container = new Container();

        container.AddNightHeavenEventBus(
            cfg =>
            {
                cfg.IdleSleepMs = 7;
                cfg.IdleCpuEnabled = false;
            }
        );

        var cfg = container.Resolve<GameLoopConfig>();

        Assert.Equal(7, cfg.IdleSleepMs);
        Assert.False(cfg.IdleCpuEnabled);
    }

    [Fact]
    public void AddNightHeavenEventBus_NoCustomConfig_AppliesDefaults()
    {
        var container = new Container();

        container.AddNightHeavenEventBus();

        var cfg = container.Resolve<GameLoopConfig>();

        Assert.True(cfg.IdleCpuEnabled);
        Assert.Equal(1, cfg.IdleSleepMs);
    }

    [Fact]
    public void AddNightHeavenEventBus_RegistersBusAndGameLoop()
    {
        var container = new Container();

        container.AddNightHeavenEventBus();

        Assert.NotNull(container.Resolve<IEventBusService>());
        Assert.NotNull(container.Resolve<IGameLoopService>());
    }

    [Fact]
    public void AddTickEventHandler_RegistersHandlerAndDrainInvokesIt()
    {
        var timeline = new List<string>();
        var container = new Container();
        container.RegisterInstance(timeline);
        container.AddNightHeavenEventBus();
        container.AddTickEventHandler<NamedTickHandler, TestTickEvent>();

        var bus = container.Resolve<IEventBusService>();
        bus.Publish(new TestTickEvent(11));

        var processed = bus.DrainTickEvents(10);

        Assert.Equal(1, processed);
        Assert.Equal(new[] { "tick:Named:11" }, timeline);
    }
}
