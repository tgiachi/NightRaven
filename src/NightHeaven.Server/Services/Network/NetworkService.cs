using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using NightHeaven.Core.Utils;
using NightHeaven.Hosting.Data.Metrics;
using NightHeaven.Hosting.Data.Network;
using NightHeaven.Hosting.Interfaces.Metrics;
using NightHeaven.Hosting.Interfaces.Services;
using NightHeaven.Hosting.Types.Metrics;
using NightHeaven.Network.Events;
using NightHeaven.Network.Server;
using NightHeaven.Network.UO.Data.Packets;
using NightHeaven.Network.UO.Registry;
using NightHeaven.Network.UO.Types.Packets;
using NightHeaven.Server.Data.Events;
using NightHeaven.Server.Interfaces.Network;
using NightHeaven.Server.Services.Network.Internal;
using Serilog;
using ILogger = Serilog.ILogger;

namespace NightHeaven.Server.Services.Network;

/// <summary>
/// Owns the TCP game listeners (one per local interface), the UDP ping echo server and the
/// background ingress thread that parses inbound bytes into packets and republishes them as
/// tick events on the event bus.
/// </summary>
public sealed class NetworkService : INetworkService, IMetricProvider, IDisposable
{
    private const int IngressIdleWaitMs = 5;

    private readonly ILogger _logger = Log.ForContext<NetworkService>();
    private readonly IEventBusService _eventBus;
    private readonly ISessionService _sessions;
    private readonly PacketRegistry _packetRegistry;
    private readonly NetworkConfig _config;

    private readonly List<NightHeavenTCPServer> _tcpServers = [];
    private readonly ConcurrentQueue<PendingClientData> _pendingClientDataQueue = new();
    private readonly ConcurrentDictionary<long, NetworkParserSessionMetrics> _parserMetrics = new();
    private readonly AutoResetEvent _pendingClientDataSignal = new(false);

    private NightHeavenUDPServer? _pingServer;
    private Thread? _ingressThread;
    private volatile bool _ingressStopRequested;
    private long _ingressQueueDepth;

    public NetworkService(
        IEventBusService eventBus,
        ISessionService sessions,
        PacketRegistry packetRegistry,
        NetworkConfig config
    )
    {
        _eventBus = eventBus;
        _sessions = sessions;
        _packetRegistry = packetRegistry;
        _config = config;
    }

    public int ConnectedSessionCount => _sessions.Count;

    public string Prefix => "network";

    public IReadOnlyList<MetricSample> Collect()
    {
        long receivedBytes = 0;
        long parsedPackets = 0;
        long unknownOpcodeDrops = 0;
        long parserErrors = 0;

        foreach (var metrics in _parserMetrics.Values)
        {
            receivedBytes += metrics.ReceivedBytes;
            parsedPackets += metrics.ParsedPackets;
            unknownOpcodeDrops += metrics.UnknownOpcodeDrops;
            parserErrors += metrics.UnknownOpcodeDrops +
                            metrics.InvalidLengthDrops +
                            metrics.ParseFailures +
                            metrics.PendingBufferOverflows;
        }

        return
        [
            new(
                "active_sessions",
                _sessions.Count,
                Help: "Currently connected sessions"
            ),
            new(
                "ingress_queue_depth",
                Interlocked.Read(ref _ingressQueueDepth),
                Help: "Pending client data items awaiting parsing"
            ),
            new(
                "received_bytes_total",
                receivedBytes,
                MetricType.Counter,
                Help: "Total bytes received across sessions"
            ),
            new(
                "parsed_packets_total",
                parsedPackets,
                MetricType.Counter,
                Help: "Total packets parsed across sessions"
            ),
            new(
                "unknown_opcode_drops_total",
                unknownOpcodeDrops,
                MetricType.Counter,
                Help: "Total bytes dropped for unknown opcodes"
            ),
            new(
                "parser_errors_total",
                parserErrors,
                MetricType.Counter,
                Help: "Total parser errors across sessions"
            )
        ];
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        StartIngressLoop();
        StartPingServer(cancellationToken);
        StartTcpServers(cancellationToken);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        for (var i = _tcpServers.Count - 1; i >= 0; i--)
        {
            await _tcpServers[i].StopAsync(cancellationToken);
            await _tcpServers[i].DisposeAsync();
        }

        _tcpServers.Clear();

        if (_pingServer is not null)
        {
            await _pingServer.StopAsync(cancellationToken);
            await _pingServer.DisposeAsync();
            _pingServer = null;
        }

        StopIngressLoop();

        _sessions.Clear();
        _parserMetrics.Clear();

        while (_pendingClientDataQueue.TryDequeue(out _))
        {
            Interlocked.Decrement(ref _ingressQueueDepth);
        }
    }

    private void StartTcpServers(CancellationToken cancellationToken)
    {
        foreach (var endPoint in NetworkUtils.GetListeningAddresses(new(IPAddress.Any, _config.Port)))
        {
            var server = new NightHeavenTCPServer(new(endPoint.Address, _config.Port));
            server.OnClientConnect += OnClientConnected;
            server.OnClientDisconnect += OnClientDisconnected;
            server.OnDataReceived += OnClientData;
            server.OnException += OnClientException;

            _tcpServers.Add(server);
            _ = server.StartAsync(cancellationToken);
            _logger.Information("TCP game server listening on {Address}:{Port}", endPoint.Address, _config.Port);
        }
    }

    private void StartPingServer(CancellationToken cancellationToken)
    {
        if (!_config.PingServerEnabled || _config.PingServerPort <= 0)
        {
            return;
        }

        _pingServer = new(new(IPAddress.Any, _config.PingServerPort));
        _ = _pingServer.StartAsync(cancellationToken);
    }

    private void OnClientConnected(object? sender, NightHeavenTCPClientEventArgs e)
    {
        var session = _sessions.GetOrCreate(e.Client);
        _parserMetrics.TryAdd(session.SessionId, new());

        _logger.Information(
            "Client connected. SessionId={SessionId}, RemoteEndPoint={RemoteEndPoint}",
            session.SessionId,
            e.Client.RemoteEndPoint
        );

        _eventBus.Publish(
            new PlayerConnectedEvent(session.SessionId, e.Client.RemoteEndPoint?.ToString(), DateTimeOffset.UtcNow)
        );
    }

    private void OnClientDisconnected(object? sender, NightHeavenTCPClientEventArgs e)
    {
        var remoteEndPoint = e.Client.RemoteEndPoint?.ToString();
        _sessions.Remove(e.Client.SessionId);
        _parserMetrics.TryRemove(e.Client.SessionId, out _);

        _logger.Information(
            "Client disconnected. SessionId={SessionId}, RemoteEndPoint={RemoteEndPoint}",
            e.Client.SessionId,
            remoteEndPoint
        );

        _eventBus.Publish(new PlayerDisconnectedEvent(e.Client.SessionId, remoteEndPoint, DateTimeOffset.UtcNow));
    }

    private void OnClientData(object? sender, NightHeavenTCPDataReceivedEventArgs e)
    {
        if (e.Data.IsEmpty)
        {
            return;
        }

        _pendingClientDataQueue.Enqueue(new(e.Client.SessionId, e.Data.ToArray()));
        Interlocked.Increment(ref _ingressQueueDepth);
        _pendingClientDataSignal.Set();
    }

    private void OnClientException(object? sender, NightHeavenTCPExceptionEventArgs e)
        => _logger.Error(e.Exception, "Client network exception");

    private void StartIngressLoop()
    {
        if (_ingressThread is not null)
        {
            return;
        }

        _ingressStopRequested = false;
        _ingressThread = new(RunIngressLoop)
        {
            IsBackground = true,
            Name = "NightHeaven-NetworkIngress"
        };
        _ingressThread.Start();
    }

    private void StopIngressLoop()
    {
        if (_ingressThread is null)
        {
            return;
        }

        _ingressStopRequested = true;
        _pendingClientDataSignal.Set();
        _ingressThread.Join(TimeSpan.FromSeconds(2));
        _ingressThread = null;
    }

    private void RunIngressLoop()
    {
        while (!_ingressStopRequested)
        {
            var processed = 0;

            while (processed < _config.MaxPacketsPerDrain && _pendingClientDataQueue.TryDequeue(out var pending))
            {
                Interlocked.Decrement(ref _ingressQueueDepth);
                processed++;

                try
                {
                    ProcessClientData(pending.SessionId, pending.Data);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unhandled exception in network ingress loop");
                }
            }

            if (processed == 0)
            {
                _pendingClientDataSignal.WaitOne(IngressIdleWaitMs);
            }
        }
    }

    private void ProcessClientData(long sessionId, byte[] data)
    {
        if (!_sessions.TryGet(sessionId, out var session))
        {
            return;
        }

        var metrics = _parserMetrics.GetOrAdd(sessionId, static _ => new());
        metrics.AddReceivedBytes(data.Length);

        session.WithPendingBytes(
            pendingBytes =>
            {
                pendingBytes.AddRange(data);

                if (pendingBytes.Count > _config.MaxPendingBufferBytes)
                {
                    metrics.IncrementPendingBufferOverflows();
                    _logger.Warning(
                        "Session {SessionId} exceeded pending buffer limit; clearing buffer",
                        sessionId
                    );
                    pendingBytes.Clear();

                    return;
                }

                ParseAvailablePackets(session, pendingBytes, metrics);
            }
        );
    }

    private void ParseAvailablePackets(GameSession session, List<byte> pendingBytes, NetworkParserSessionMetrics metrics)
    {
        while (pendingBytes.Count > 0)
        {
            var opCode = pendingBytes[0];

            if (!_packetRegistry.TryGetDescriptor(opCode, out var descriptor))
            {
                // Unknown opcode: we cannot know the length, so drop the whole buffer to resync.
                metrics.IncrementUnknownOpcodeDrops();
                _logger.Warning(
                    "Unknown opcode 0x{OpCode:X2} from session {SessionId}; dropping {Count} buffered bytes",
                    opCode,
                    session.SessionId,
                    pendingBytes.Count
                );
                pendingBytes.Clear();

                return;
            }

            var length = ResolvePacketLength(pendingBytes, descriptor);

            if (length is null)
            {
                // Need more bytes to determine the length.
                return;
            }

            if (length.Value <= 0 || length.Value > _config.MaxDeclaredPacketLength)
            {
                metrics.IncrementInvalidLengthDrops();
                _logger.Warning(
                    "Invalid declared length {Length} for opcode 0x{OpCode:X2} from session {SessionId}; dropping buffer",
                    length.Value,
                    opCode,
                    session.SessionId
                );
                pendingBytes.Clear();

                return;
            }

            if (pendingBytes.Count < length.Value)
            {
                // Full packet not yet available.
                return;
            }

            var rawPacket = new byte[length.Value];
            pendingBytes.CopyTo(0, rawPacket, 0, length.Value);
            pendingBytes.RemoveRange(0, length.Value);

            if (!_packetRegistry.TryCreatePacket(opCode, out var packet) || packet is null)
            {
                metrics.IncrementUnknownOpcodeDrops();

                continue;
            }

            if (!packet.TryParse(rawPacket))
            {
                metrics.IncrementParseFailures();
                _logger.Warning(
                    "Failed to parse packet 0x{OpCode:X2} from session {SessionId}",
                    opCode,
                    session.SessionId
                );

                continue;
            }

            metrics.IncrementParsedPackets();
            _eventBus.Publish(new PacketReceivedEvent(session.SessionId, opCode, packet, DateTimeOffset.UtcNow));
        }
    }

    private static int? ResolvePacketLength(List<byte> pendingBytes, PacketDescriptor descriptor)
    {
        if (descriptor.Sizing == PacketSizing.Fixed)
        {
            return descriptor.Length;
        }

        if (pendingBytes.Count < 3)
        {
            return null;
        }

        Span<byte> lengthBuffer = [pendingBytes[1], pendingBytes[2]];

        return BinaryPrimitives.ReadUInt16BigEndian(lengthBuffer);
    }

    private readonly record struct PendingClientData(long SessionId, byte[] Data);

    public void Dispose()
    {
        StopIngressLoop();
        _pendingClientDataSignal.Dispose();
    }
}
