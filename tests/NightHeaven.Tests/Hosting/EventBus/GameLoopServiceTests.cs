using Microsoft.Extensions.DependencyInjection;
using NightHeaven.Hosting.Data;
using NightHeaven.Hosting.Interfaces;
using NightHeaven.Server.Services.EventBus;
using NightHeaven.Server.Services.GameLoop;
using NightHeaven.Tests.Hosting.EventBus.Support;

namespace NightHeaven.Tests.Hosting.EventBus;

public class GameLoopServiceTests
{
    [Fact]
    public async Task StartStop_StartsAndJoinsThread()
    {
        var (_, loop) = Build(_ => { });

        await loop.StartAsync(CancellationToken.None);
        await loop.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task RunLoop_DrainsPublishedTickEvents()
    {
        var timeline = new List<string>();
        var (bus, loop) = Build(services =>
        {
            services.AddSingleton<ITickEventHandler<TestTickEvent>>(
                _ => new TimelineTickHandler("A", timeline)
            );
        });

        await loop.StartAsync(CancellationToken.None);

        bus.Publish(new TestTickEvent(1));
        bus.Publish(new TestTickEvent(2));
        bus.Publish(new TestTickEvent(3));

        await WaitForAsync(() =>
        {
            lock (timeline)
            {
                return timeline.Count >= 3;
            }
        }, TimeSpan.FromSeconds(2));

        await loop.StopAsync(CancellationToken.None);

        Assert.Equal(new[] { "tick:A:1", "tick:A:2", "tick:A:3" }, timeline);
        Assert.True(loop.TickCount > 0);
    }

    [Fact]
    public async Task RunLoop_IdleWhenNoWork_DoesNotSpinAtFullCpu()
    {
        var (_, loop) = Build(_ => { }, new GameLoopConfig { IdleSleepMs = 5 });

        await loop.StartAsync(CancellationToken.None);
        await Task.Delay(50);
        await loop.StopAsync(CancellationToken.None);

        // With 5 ms idle sleep, 50 ms should yield ~10 ticks max, not millions.
        Assert.InRange(loop.TickCount, 1, 200);
    }

    [Fact]
    public async Task StopAsync_DrainsResidualQueueGracefully()
    {
        var timeline = new List<string>();
        var (bus, loop) = Build(services =>
        {
            services.AddSingleton<ITickEventHandler<TestTickEvent>>(
                _ => new TimelineTickHandler("A", timeline)
            );
        });

        await loop.StartAsync(CancellationToken.None);
        bus.Publish(new TestTickEvent(99));

        await WaitForAsync(() =>
        {
            lock (timeline)
            {
                return timeline.Count == 1;
            }
        }, TimeSpan.FromSeconds(2));

        await loop.StopAsync(CancellationToken.None);

        Assert.Equal(new[] { "tick:A:99" }, timeline);
    }

    private static (EventBusService bus, GameLoopService loop) Build(
        Action<ServiceCollection> configure,
        GameLoopConfig? config = null
    )
    {
        var services = new ServiceCollection();
        configure(services);
        var sp = services.BuildServiceProvider();
        var bus = new EventBusService(sp);
        var loop = new GameLoopService(bus, config ?? new GameLoopConfig());
        return (bus, loop);
    }

    private static async Task WaitForAsync(Func<bool> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(10);
        }

        throw new TimeoutException($"Condition not met within {timeout}.");
    }
}
