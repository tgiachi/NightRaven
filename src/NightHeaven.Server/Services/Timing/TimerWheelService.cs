using NightHeaven.Hosting.Data.Metrics;
using NightHeaven.Hosting.Data.Timing;
using NightHeaven.Hosting.Interfaces.Metrics;
using NightHeaven.Hosting.Interfaces.Timing;
using NightHeaven.Server.Services.Timing.Internal;
using Serilog;
using ILogger = Serilog.ILogger;

namespace NightHeaven.Server.Services.Timing;

/// <summary>
/// Hashed timer wheel driven by <see cref="ITimerService.UpdateTicksDelta" /> from the game loop.
/// </summary>
public sealed class TimerWheelService : ITimerService, IMetricProvider
{
    private readonly ILogger _logger = Log.ForContext<TimerWheelService>();
    private readonly TimeSpan _tickDuration;
    private readonly double _tickDurationMs;
    private readonly LinkedList<TimerEntry>[] _wheel;
    private readonly Lock _syncRoot = new();
    private readonly Dictionary<string, TimerEntry> _timersById = new(StringComparer.Ordinal);
    private readonly Dictionary<string, HashSet<string>> _timerIdsByName = new(StringComparer.Ordinal);

    private long _currentTick;
    private long _lastTimestampMilliseconds = -1;
    private double _accumulatedMilliseconds;

    private long _totalRegistered;
    private long _totalExecuted;
    private long _callbackErrors;
    private long _totalCallbackElapsedStopwatchTicks;
    private long _totalProcessedTicks;

    public string Prefix => "timer";

    public TimerWheelService(TimerWheelConfig config)
    {
        if (config.TickDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(config),
                "TimerWheelConfig.TickDuration must be positive."
            );
        }

        if (config.WheelSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(config),
                "TimerWheelConfig.WheelSize must be positive."
            );
        }

        _tickDuration = config.TickDuration;
        _tickDurationMs = _tickDuration.TotalMilliseconds;
        _wheel = new LinkedList<TimerEntry>[config.WheelSize];

        for (var i = 0; i < _wheel.Length; i++)
        {
            _wheel[i] = new LinkedList<TimerEntry>();
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        UnregisterAllTimers();

        return Task.CompletedTask;
    }

    public string RegisterTimer(
        string name,
        TimeSpan interval,
        Action callback,
        TimeSpan? delay = null,
        bool repeat = false
    )
        => throw new NotImplementedException();

    public bool UnregisterTimer(string timerId)
        => throw new NotImplementedException();

    public int UnregisterTimersByName(string name)
        => throw new NotImplementedException();

    public void UnregisterAllTimers()
    {
        lock (_syncRoot)
        {
            _timersById.Clear();
            _timerIdsByName.Clear();

            foreach (var bucket in _wheel)
            {
                bucket.Clear();
            }
        }
    }

    public int UpdateTicksDelta(long timestampMilliseconds)
        => throw new NotImplementedException();

    public IReadOnlyList<MetricSample> Collect()
        => throw new NotImplementedException();
}
