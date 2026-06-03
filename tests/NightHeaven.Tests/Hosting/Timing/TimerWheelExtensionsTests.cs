using global::DryIoc;
using NightHeaven.Hosting.Data.Timing;
using NightHeaven.Hosting.Interfaces.Timing;
using NightHeaven.Server.Extensions.DryIoc;

namespace NightHeaven.Tests.Hosting.Timing;

public class TimerWheelExtensionsTests
{
    [Fact]
    public void AddNightHeavenTimerWheel_AppliesCustomConfig()
    {
        var container = new Container();
        container.AddNightHeavenTimerWheel(
            cfg =>
            {
                cfg.TickDuration = TimeSpan.FromMilliseconds(4);
                cfg.WheelSize = 1024;
            }
        );

        var cfg = container.Resolve<TimerWheelConfig>();
        Assert.Equal(TimeSpan.FromMilliseconds(4), cfg.TickDuration);
        Assert.Equal(1024, cfg.WheelSize);
    }

    [Fact]
    public void AddNightHeavenTimerWheel_DefaultConfig_HasExpectedValues()
    {
        var container = new Container();
        container.AddNightHeavenTimerWheel();

        var cfg = container.Resolve<TimerWheelConfig>();
        Assert.Equal(TimeSpan.FromMilliseconds(8), cfg.TickDuration);
        Assert.Equal(512, cfg.WheelSize);
    }

    [Fact]
    public void AddNightHeavenTimerWheel_RegistersServiceAndConfig()
    {
        var container = new Container();
        container.AddNightHeavenTimerWheel();

        Assert.NotNull(container.Resolve<ITimerService>());
        Assert.NotNull(container.Resolve<TimerWheelConfig>());
    }
}
