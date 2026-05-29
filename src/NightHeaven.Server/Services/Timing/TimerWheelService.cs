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
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Timer name cannot be empty.", nameof(name));
        }

        if (interval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(interval), "Interval must be positive.");
        }

        ArgumentNullException.ThrowIfNull(callback);

        var dueTime = delay ?? interval;

        if (dueTime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delay), "Delay must be positive.");
        }

        var entry = new TimerEntry
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = name,
            Callback = callback,
            Interval = interval,
            Repeat = repeat
        };

        lock (_syncRoot)
        {
            _timersById[entry.Id] = entry;
            Interlocked.Increment(ref _totalRegistered);

            if (!_timerIdsByName.TryGetValue(name, out var ids))
            {
                ids = [];
                _timerIdsByName[name] = ids;
            }

            ids.Add(entry.Id);
            ScheduleEntry(entry, dueTime);
        }

        return entry.Id;
    }

    public bool UnregisterTimer(string timerId)
    {
        if (string.IsNullOrWhiteSpace(timerId))
        {
            return false;
        }

        lock (_syncRoot)
        {
            return RemoveEntryById(timerId);
        }
    }

    public int UnregisterTimersByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return 0;
        }

        lock (_syncRoot)
        {
            if (!_timerIdsByName.TryGetValue(name, out var ids) || ids.Count == 0)
            {
                return 0;
            }

            var timerIds = ids.ToArray();
            var removed = 0;

            foreach (var timerId in timerIds)
            {
                if (RemoveEntryById(timerId))
                {
                    removed++;
                }
            }

            return removed;
        }
    }

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

    private bool RemoveEntryById(string timerId)
    {
        if (!_timersById.TryGetValue(timerId, out var entry))
        {
            return false;
        }

        entry.Cancelled = true;

        if (entry.Node is not null)
        {
            _wheel[entry.SlotIndex].Remove(entry.Node);
            entry.Node = null;
        }

        RemoveFromIndexes(entry);

        return true;
    }

    private void RemoveFromIndexes(TimerEntry entry)
    {
        _timersById.Remove(entry.Id);

        if (!_timerIdsByName.TryGetValue(entry.Name, out var ids))
        {
            return;
        }

        ids.Remove(entry.Id);

        if (ids.Count == 0)
        {
            _timerIdsByName.Remove(entry.Name);
        }
    }

    private void ScheduleEntry(TimerEntry entry, TimeSpan dueTime)
    {
        var ticks = ToWheelTicks(dueTime);
        var targetTick = _currentTick + ticks;
        var slotIndex = (int)(targetTick % _wheel.Length);
        var rounds = (ticks - 1) / _wheel.Length;

        entry.SlotIndex = slotIndex;
        entry.RemainingRounds = rounds;
        entry.Cancelled = false;
        entry.Node = _wheel[slotIndex].AddLast(entry);
    }

    private long ToWheelTicks(TimeSpan dueTime)
    {
        var ticks = (long)Math.Ceiling(dueTime.TotalMilliseconds / _tickDurationMs);

        return Math.Max(1, ticks);
    }
}
