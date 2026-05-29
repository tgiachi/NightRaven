using NightHeaven.Hosting.Data.Timing;
using NightHeaven.Server.Services.Timing;

namespace NightHeaven.Tests.Hosting.Timing;

public class TimerWheelServiceTests
{
    [Fact]
    public void Ctor_ZeroTickDuration_Throws()
    {
        var cfg = new TimerWheelConfig { TickDuration = TimeSpan.Zero };
        Assert.Throws<ArgumentOutOfRangeException>(() => new TimerWheelService(cfg));
    }

    [Fact]
    public void Ctor_NegativeTickDuration_Throws()
    {
        var cfg = new TimerWheelConfig { TickDuration = TimeSpan.FromMilliseconds(-1) };
        Assert.Throws<ArgumentOutOfRangeException>(() => new TimerWheelService(cfg));
    }

    [Fact]
    public void Ctor_ZeroWheelSize_Throws()
    {
        var cfg = new TimerWheelConfig { TickDuration = TimeSpan.FromMilliseconds(8), WheelSize = 0 };
        Assert.Throws<ArgumentOutOfRangeException>(() => new TimerWheelService(cfg));
    }

    [Fact]
    public void Ctor_ValidConfig_DoesNotThrow()
    {
        var cfg = new TimerWheelConfig { TickDuration = TimeSpan.FromMilliseconds(8), WheelSize = 16 };
        _ = new TimerWheelService(cfg);
    }
}
