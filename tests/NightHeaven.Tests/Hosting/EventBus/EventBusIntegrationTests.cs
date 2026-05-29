using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NightHeaven.Hosting.Interfaces.EventHandlers;
using NightHeaven.Hosting.Interfaces.Events;
using NightHeaven.Hosting.Interfaces.Services;
using NightHeaven.Server.Extensions;
using NightHeaven.Tests.Hosting.EventBus.Support;

namespace NightHeaven.Tests.Hosting.EventBus;

public class EventBusIntegrationTests
{
    [Fact]
    public async Task FullHost_PublishTickEvent_HandlerInvokedThroughGameLoop()
    {
        var timeline = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(timeline);
        services.AddNightHeavenEventBus();
        services.AddTickEventHandler<IntegrationTickHandler, TestTickEvent>();

        var sp = services.BuildServiceProvider();
        var orchestrator = sp.GetRequiredService<IEnumerable<IHostedService>>().Single();
        var bus = sp.GetRequiredService<IEventBusService>();

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

    [Fact]
    public async Task FullHost_PublishAsyncEvent_HandlerInvokedWithoutGameLoop()
    {
        var timeline = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(timeline);
        services.AddNightHeavenEventBus();
        services.AddAsyncEventHandler<IntegrationAsyncHandler, TestAsyncEvent>();

        var sp = services.BuildServiceProvider();
        var orchestrator = sp.GetRequiredService<IEnumerable<IHostedService>>().Single();
        var bus = sp.GetRequiredService<IEventBusService>();

        await orchestrator.StartAsync(CancellationToken.None);

        await bus.PublishAsync(new TestAsyncEvent("integration"));

        await orchestrator.StopAsync(CancellationToken.None);

        Assert.Equal(new[] { "async:Integration:integration" }, timeline);
    }

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
}
