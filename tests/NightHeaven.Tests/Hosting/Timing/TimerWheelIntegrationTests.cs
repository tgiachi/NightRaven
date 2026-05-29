using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NightHeaven.Hosting.Interfaces.Timing;
using NightHeaven.Server.Extensions;

namespace NightHeaven.Tests.Hosting.Timing;

public class TimerWheelIntegrationTests
{
    [Fact]
    public async Task FullHost_TimerRegisteredAfterStart_FiresThroughGameLoop()
    {
        var services = new ServiceCollection();
        services.AddNightHeavenEventBus();
        services.AddNightHeavenTimerWheel();

        var sp = services.BuildServiceProvider();
        var orchestrator = sp.GetRequiredService<IEnumerable<IHostedService>>().Single();
        var timers = sp.GetRequiredService<ITimerService>();

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
