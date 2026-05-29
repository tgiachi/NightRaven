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

    [Fact]
    public void RegisterTimer_EmptyName_Throws()
    {
        var svc = NewService();
        Assert.Throws<ArgumentException>(
            () => svc.RegisterTimer("", TimeSpan.FromMilliseconds(8), () => { })
        );
    }

    [Fact]
    public void RegisterTimer_NonPositiveInterval_Throws()
    {
        var svc = NewService();
        Assert.Throws<ArgumentOutOfRangeException>(
            () => svc.RegisterTimer("x", TimeSpan.Zero, () => { })
        );
    }

    [Fact]
    public void RegisterTimer_NullCallback_Throws()
    {
        var svc = NewService();
        Assert.Throws<ArgumentNullException>(
            () => svc.RegisterTimer("x", TimeSpan.FromMilliseconds(8), null!)
        );
    }

    [Fact]
    public void RegisterTimer_ReturnsNonEmptyId()
    {
        var svc = NewService();
        var id = svc.RegisterTimer("x", TimeSpan.FromMilliseconds(8), () => { });
        Assert.False(string.IsNullOrEmpty(id));
    }

    [Fact]
    public void RegisterTimer_DistinctRegistrations_GetDistinctIds()
    {
        var svc = NewService();
        var a = svc.RegisterTimer("x", TimeSpan.FromMilliseconds(8), () => { });
        var b = svc.RegisterTimer("x", TimeSpan.FromMilliseconds(8), () => { });
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void UnregisterTimer_ExistingId_ReturnsTrue()
    {
        var svc = NewService();
        var id = svc.RegisterTimer("x", TimeSpan.FromMilliseconds(8), () => { });
        Assert.True(svc.UnregisterTimer(id));
    }

    [Fact]
    public void UnregisterTimer_UnknownId_ReturnsFalse()
    {
        var svc = NewService();
        Assert.False(svc.UnregisterTimer("nope"));
    }

    [Fact]
    public void UnregisterTimer_EmptyId_ReturnsFalse()
    {
        var svc = NewService();
        Assert.False(svc.UnregisterTimer(""));
    }

    [Fact]
    public void UnregisterTimersByName_RemovesEveryTimerWithThatName()
    {
        var svc = NewService();
        svc.RegisterTimer("group", TimeSpan.FromMilliseconds(8), () => { });
        svc.RegisterTimer("group", TimeSpan.FromMilliseconds(8), () => { });
        svc.RegisterTimer("other", TimeSpan.FromMilliseconds(8), () => { });

        var removed = svc.UnregisterTimersByName("group");

        Assert.Equal(2, removed);
        Assert.Equal(0, svc.UnregisterTimersByName("group"));
        Assert.Equal(1, svc.UnregisterTimersByName("other"));
    }

    [Fact]
    public void UnregisterAllTimers_ClearsState()
    {
        var svc = NewService();
        svc.RegisterTimer("a", TimeSpan.FromMilliseconds(8), () => { });
        svc.RegisterTimer("b", TimeSpan.FromMilliseconds(8), () => { });

        svc.UnregisterAllTimers();

        Assert.Equal(0, svc.UnregisterTimersByName("a"));
        Assert.Equal(0, svc.UnregisterTimersByName("b"));
    }

    private static TimerWheelService NewService(int tickDurationMs = 8, int wheelSize = 16)
        => new(new TimerWheelConfig
        {
            TickDuration = TimeSpan.FromMilliseconds(tickDurationMs),
            WheelSize = wheelSize
        });
}
