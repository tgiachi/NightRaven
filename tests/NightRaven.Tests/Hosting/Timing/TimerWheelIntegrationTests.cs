using DryIoc;
using NightRaven.Abstractions.Interfaces.Timing;
using NightRaven.Server.Extensions.Configuration;
using NightRaven.Server.Extensions.EventBus;
using NightRaven.Server.Extensions.Timing;
using NightRaven.Tests.Support;

namespace NightRaven.Tests.Hosting.Timing;

public class TimerWheelIntegrationTests : IDisposable
{
    private readonly string _dir = Path.Combine(
        Path.GetTempPath(),
        $"nightraven-timerwheel-integration-{Guid.NewGuid():N}"
    );

    private string ConfigPath => Path.Combine(_dir, "nightraven.toml");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task FullHost_TimerRegisteredAfterStart_FiresThroughGameLoop()
    {
        var container = new Container();
        container.AddNightRavenEventBus();
        container.AddNightRavenTimerWheel();
        container.AddNightRavenConfig(ConfigPath);

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
