using global::DryIoc;
using NightHeaven.Hosting.Interfaces.EventHandlers;
using NightHeaven.Hosting.Interfaces.Services;
using NightHeaven.Server.Extensions.DryIoc;
using NightHeaven.Tests.Hosting.EventBus.Support;
using NightHeaven.Tests.Support;

namespace NightHeaven.Tests.Hosting.EventBus;

public class EventBusIntegrationTests : IDisposable
{
    private readonly string _dir = Path.Combine(
        Path.GetTempPath(),
        $"nightheaven-eventbus-integration-{Guid.NewGuid():N}"
    );

    private string ConfigPath => Path.Combine(_dir, "nightheaven.toml");

    private sealed class IntegrationTickHandler : ITickEventHandler<TestTickEvent>
    {
        private readonly List<string> _timeline;

        public IntegrationTickHandler(List<string> timeline)
        {
            _timeline = timeline;
        }

        public void Handle(TestTickEvent evt)
        {
            lock (_timeline)
            {
                _timeline.Add($"tick:Integration:{evt.Value}");
            }
        }
    }

    private sealed class IntegrationAsyncHandler : IAsyncEventHandler<TestAsyncEvent>
    {
        private readonly List<string> _timeline;

        public IntegrationAsyncHandler(List<string> timeline)
        {
            _timeline = timeline;
        }

        public Task HandleAsync(TestAsyncEvent evt, CancellationToken cancellationToken)
        {
            lock (_timeline)
            {
                _timeline.Add($"async:Integration:{evt.Payload}");
            }

            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task FullHost_PublishAsyncEvent_HandlerInvokedWithoutGameLoop()
    {
        var timeline = new List<string>();
        var container = new Container();
        container.RegisterInstance(timeline);
        container.AddNightHeavenEventBus();
        container.AddNightHeavenConfig(ConfigPath);
        container.AddAsyncEventHandler<IntegrationAsyncHandler, TestAsyncEvent>();

        var orchestrator = container.Orchestrator();
        var bus = container.Resolve<IEventBusService>();

        await orchestrator.StartAsync(CancellationToken.None);

        await bus.PublishAsync(new TestAsyncEvent("integration"));

        await orchestrator.StopAsync(CancellationToken.None);

        Assert.Equal(new[] { "async:Integration:integration" }, timeline);
    }

    [Fact]
    public async Task FullHost_PublishTickEvent_HandlerInvokedThroughGameLoop()
    {
        var timeline = new List<string>();
        var container = new Container();
        container.RegisterInstance(timeline);
        container.AddNightHeavenEventBus();
        container.AddNightHeavenConfig(ConfigPath);
        container.AddTickEventHandler<IntegrationTickHandler, TestTickEvent>();

        var orchestrator = container.Orchestrator();
        var bus = container.Resolve<IEventBusService>();

        await orchestrator.StartAsync(CancellationToken.None);

        bus.Publish(new TestTickEvent(42));

        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);

        while (DateTime.UtcNow < deadline)
        {
            lock (timeline)
            {
                if (timeline.Count == 1)
                {
                    break;
                }
            }
            await Task.Delay(10);
        }

        await orchestrator.StopAsync(CancellationToken.None);

        Assert.Equal(new[] { "tick:Integration:42" }, timeline);
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
