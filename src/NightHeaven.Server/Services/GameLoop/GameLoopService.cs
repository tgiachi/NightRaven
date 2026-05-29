using System.Diagnostics;
using NightHeaven.Hosting.Data;
using NightHeaven.Hosting.Interfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace NightHeaven.Server.Services.GameLoop;

/// <summary>
/// Owns the dedicated game-loop thread. Drains tick events from <see cref="IEventBusService" />,
/// sleeps when idle, exposes basic metrics.
/// </summary>
public sealed class GameLoopService : IGameLoopService, IDisposable
{
    private const int MaxTickEventsPerFrame = 256;
    private const double SlowTickThresholdMs = 250;

    private readonly ILogger _logger = Log.ForContext<GameLoopService>();
    private readonly IEventBusService _bus;
    private readonly GameLoopConfig _config;
    private readonly CancellationTokenSource _cts = new();
    private readonly Lock _metricsSync = new();

    private Thread? _thread;
    private long _tickCount;
    private double _averageTickMs;
    private double _maxTickMs;
    private long _idleSleepCount;

    public GameLoopService(IEventBusService bus, GameLoopConfig config)
    {
        _bus = bus;
        _config = config;
    }

    public long TickCount => Interlocked.Read(ref _tickCount);

    public double AverageTickMs
    {
        get
        {
            lock (_metricsSync)
            {
                return _averageTickMs;
            }
        }
    }

    public double MaxTickMs
    {
        get
        {
            lock (_metricsSync)
            {
                return _maxTickMs;
            }
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _thread = new Thread(RunLoop)
        {
            IsBackground = true,
            Name = "NightHeaven-GameLoop"
        };
        _thread.Start();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts.Cancel();
        _thread?.Join(TimeSpan.FromSeconds(5));

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }

    private void RunLoop()
    {
        while (!_cts.IsCancellationRequested)
        {
            var tickStart = Stopwatch.GetTimestamp();
            var workUnits = _bus.DrainTickEvents(MaxTickEventsPerFrame);
            var elapsed = Stopwatch.GetElapsedTime(tickStart);

            UpdateMetrics(elapsed);

            if (elapsed.TotalMilliseconds >= SlowTickThresholdMs)
            {
                _logger.Warning(
                    "Slow tick: {Elapsed:0.###}ms workUnits={WorkUnits} queueDepth={Queue}",
                    elapsed.TotalMilliseconds,
                    workUnits,
                    _bus.CurrentTickQueueDepth
                );
            }

            if (_config.IdleCpuEnabled && workUnits == 0)
            {
                Thread.Sleep(_config.IdleSleepMs);
                Interlocked.Increment(ref _idleSleepCount);
            }
        }
    }

    private void UpdateMetrics(TimeSpan elapsed)
    {
        Interlocked.Increment(ref _tickCount);

        lock (_metricsSync)
        {
            // Exponential moving average: 0.95 weight to history, 0.05 to current sample.
            _averageTickMs = _averageTickMs * 0.95 + elapsed.TotalMilliseconds * 0.05;
            _maxTickMs = Math.Max(_maxTickMs, elapsed.TotalMilliseconds);
        }
    }
}
