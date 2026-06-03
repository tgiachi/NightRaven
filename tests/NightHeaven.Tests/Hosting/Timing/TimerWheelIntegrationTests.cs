using global::DryIoc;
using NightHeaven.Hosting.Interfaces.Timing;
using NightHeaven.Server.Extensions.DryIoc;
using NightHeaven.Tests.Support;

namespace NightHeaven.Tests.Hosting.Timing;

public class TimerWheelIntegrationTests
{
    [Fact]
    public async Task FullHost_TimerRegisteredAfterStart_FiresThroughGameLoop()
    {
        var container = new Container();
        container.AddNightHeavenEventBus();
        container.AddNightHeavenTimerWheel();

        var orchestrator = container.Orchestrator();
        var timers = container.Resolve<ITimerService>();

        await orchestrator.StartAsync(CancellationToken.None);

        var fired = 0;
        timers.RegisterTimer("ping", TimeSpan.FromMilliseconds(50), () => Interlocked.Increment(ref fired));

        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);

        while (DateTime.UtcNow < deadline && Volatile.Read(ref fired) == 0)
        {
            await Task.Delay(10);
        }

        await orchestrator.StopAsync(CancellationToken.None);

        Assert.True(Volatile.Read(ref fired) >= 1, "timer should have fired at least once before stop");
    }
}
