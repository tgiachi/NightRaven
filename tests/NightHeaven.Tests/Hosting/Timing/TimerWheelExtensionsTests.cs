using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NightHeaven.Hosting.Data.Timing;
using NightHeaven.Hosting.Interfaces.Timing;
using NightHeaven.Server.Extensions;

namespace NightHeaven.Tests.Hosting.Timing;

public class TimerWheelExtensionsTests
{
    [Fact]
    public void AddNightHeavenTimerWheel_RegistersServiceAndConfig()
    {
        var services = new ServiceCollection();
        services.AddNightHeavenTimerWheel();

        var sp = services.BuildServiceProvider();

        Assert.NotNull(sp.GetService<ITimerService>());
        Assert.NotNull(sp.GetService<TimerWheelConfig>());
    }

    [Fact]
    public void AddNightHeavenTimerWheel_RegistersOrchestratorOnce()
    {
        var services = new ServiceCollection();
        services.AddNightHeavenTimerWheel();

        var sp = services.BuildServiceProvider();
        var hosted = sp.GetServices<IHostedService>().ToArray();

        Assert.Single(hosted);
        Assert.Equal("NightHeavenServiceOrchestrator", hosted[0].GetType().Name);
    }

    [Fact]
    public void AddNightHeavenTimerWheel_AppliesCustomConfig()
    {
        var services = new ServiceCollection();
        services.AddNightHeavenTimerWheel(cfg =>
        {
            cfg.TickDuration = TimeSpan.FromMilliseconds(4);
            cfg.WheelSize = 1024;
        });

        var cfg = services.BuildServiceProvider().GetRequiredService<TimerWheelConfig>();
        Assert.Equal(TimeSpan.FromMilliseconds(4), cfg.TickDuration);
        Assert.Equal(1024, cfg.WheelSize);
    }

    [Fact]
    public void AddNightHeavenTimerWheel_DefaultConfig_HasExpectedValues()
    {
        var services = new ServiceCollection();
        services.AddNightHeavenTimerWheel();

        var cfg = services.BuildServiceProvider().GetRequiredService<TimerWheelConfig>();
        Assert.Equal(TimeSpan.FromMilliseconds(8), cfg.TickDuration);
        Assert.Equal(512, cfg.WheelSize);
    }
}
