using Microsoft.Extensions.DependencyInjection;
using NightHeaven.Hosting.Interfaces.EventHandlers;
using NightHeaven.Hosting.Interfaces.Events;
using NightHeaven.Hosting.Interfaces.Metrics;
using NightHeaven.Hosting.Types.Metrics;
using NightHeaven.Server.Services.EventBus;
using NightHeaven.Tests.Hosting.EventBus.Support;

namespace NightHeaven.Tests.Hosting.Metrics;

public class EventBusMetricsTests
{
    [Fact]
    public void Prefix_IsBus()
    {
        var bus = BuildBus(_ => { });
        Assert.Equal("bus", ((IMetricProvider)bus).Prefix);
    }

    [Fact]
    public async Task Collect_AfterAsyncPublish_AsyncCounterIncrements()
    {
        var bus = BuildBus(_ => { });

        await bus.PublishAsync(new TestAsyncEvent("x"));

        var v = ValueOf(bus, "async_events_total");
        Assert.Equal(1, v);
    }

    [Fact]
    public void Collect_AfterTickPublish_TickCounterIncrementsAndQueueDepthShown()
    {
        var bus = BuildBus(_ => { });

        bus.Publish(new TestTickEvent(1));
        bus.Publish(new TestTickEvent(2));

        Assert.Equal(2, ValueOf(bus, "tick_events_total"));
        Assert.Equal(2, ValueOf(bus, "tick_queue_depth"));
    }

    [Fact]
    public async Task Collect_AfterAsyncHandlerThrows_ErrorCounterIncrements()
    {
        var bus = BuildBus(services =>
        {
            services.AddSingleton<IAsyncEventHandler<TestAsyncEvent>>(_ => new ThrowingAsyncHandler());
        });

        await bus.PublishAsync(new TestAsyncEvent("x"));

        Assert.Equal(1, ValueOf(bus, "handler_errors_total"));
    }

    [Fact]
    public void Collect_AfterTickHandlerThrows_ErrorCounterIncrements()
    {
        var bus = BuildBus(services =>
        {
            services.AddSingleton<ITickEventHandler<TestTickEvent>>(_ => new ThrowingTickHandler());
        });

        bus.Publish(new TestTickEvent(1));
        bus.DrainTickEvents(maxItems: 10);

        Assert.Equal(1, ValueOf(bus, "handler_errors_total"));
    }

    [Fact]
    public void Collect_CountersHaveCorrectMetricType()
    {
        var bus = BuildBus(_ => { });
        var byName = ((IMetricProvider)bus).Collect().ToDictionary(s => s.Name, s => s);

        Assert.Equal(MetricType.Counter, byName["async_events_total"].Type);
        Assert.Equal(MetricType.Counter, byName["tick_events_total"].Type);
        Assert.Equal(MetricType.Counter, byName["handler_errors_total"].Type);
        Assert.Equal(MetricType.Gauge,   byName["tick_queue_depth"].Type);
    }

    private static EventBusService BuildBus(Action<ServiceCollection> configure)
    {
        var services = new ServiceCollection();
        configure(services);
        return new EventBusService(services.BuildServiceProvider());
    }

    private static double ValueOf(EventBusService bus, string name)
        => ((IMetricProvider)bus).Collect().Single(s => s.Name == name).Value;
}
