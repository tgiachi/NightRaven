using DryIoc;
using NightRaven.Abstractions.Extensions.DryIoc;
using NightRaven.Abstractions.Interfaces.EventHandlers;
using NightRaven.Abstractions.Interfaces.Services;
using NightRaven.Server.Extensions.DryIoc;
using NightRaven.Tests.Hosting.EventBus.Support;
using NightRaven.Tests.Support;

namespace NightRaven.Tests.Hosting.EventBus;

public class EventBusIntegrationTests : IDisposable
{
    private readonly string _dir = Path.Combine(
        Path.GetTempPath(),
        $"nightraven-eventbus-integration-{Guid.NewGuid():N}"
    );

    private string ConfigPath => Path.Combine(_dir, "nightraven.toml");

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

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task FullHost_PublishAsyncEvent_HandlerInvokedWithoutGameLoop()
    {
        var timeline = new List<string>();
        var container = new Container();
        container.RegisterInstance(timeline);
        container.AddNightRavenEventBus();
        container.AddNightRavenConfig(ConfigPath);
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
        container.AddNightRavenEventBus();
        container.AddNightRavenConfig(ConfigPath);
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
}
