using System.Net;
using System.Net.Sockets;
using NightHeaven.Core.Buffers;
using NightHeaven.Network.Buffers;
using NightHeaven.Network.Events;
using NightHeaven.Network.Interfaces;
using NightHeaven.Network.Pipeline;
using Serilog;

namespace NightHeaven.Network.Client;

/// <summary>
/// Represents a connected TCP client with async send/receive loops,
/// middleware processing, lifecycle events, and recent byte history.
/// </summary>
public sealed class NightHeavenTCPClient : IAsyncDisposable, IDisposable
{
    private const int DefaultReceiveBufferSize = 8192;
    private const int DefaultHistoryBufferCapacity = 65536;

    private static long _sessionIdSequence;

    private readonly ILogger _logger = Log.ForContext<NightHeavenTCPClient>();
    private readonly NetMiddlewarePipeline _middlewarePipeline;
    private readonly Socket _socket;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly CancellationTokenSource _internalCancellationTokenSource = new();
    private readonly CircularBuffer<byte> _receiveBuffer;
    private readonly Lock _receiveBufferSync = new();

    private CancellationTokenRegistration _externalCancellationTokenRegistration;
    private Task? _receiveLoopTask;
    private int _started;
    private int _closed;

    /// <summary>
    /// Creates a client wrapper for an accepted socket.
    /// </summary>
    /// <param name="socket">Connected socket.</param>
    /// <param name="middlewares">Optional middleware list.</param>
    /// <param name="receiveBufferSize">Receive chunk size in bytes.</param>
    /// <param name="historyBufferCapacity">Max number of received bytes to keep in history.</param>
    public NightHeavenTCPClient(
        Socket socket,
        IEnumerable<INetMiddleware>? middlewares = null,
        int receiveBufferSize = DefaultReceiveBufferSize,
        int historyBufferCapacity = DefaultHistoryBufferCapacity
    )
    {
        _socket = socket;
        _middlewarePipeline = new(middlewares);
        _receiveBuffer = new(historyBufferCapacity);
        ReceiveBufferSize = receiveBufferSize;
        SessionId = Interlocked.Increment(ref _sessionIdSequence);
    }

    /// <summary>
    /// Raised when the client is fully connected and receive loop starts.
    /// </summary>
    public event EventHandler<NightHeavenTCPClientEventArgs>? OnConnected;

    /// <summary>
    /// Raised when the client is disconnected.
    /// </summary>
    public event EventHandler<NightHeavenTCPClientEventArgs>? OnDisconnected;

    /// <summary>
    /// Raised when data is received (after middleware pipeline).
    /// </summary>
    public event EventHandler<NightHeavenTCPDataReceivedEventArgs>? OnDataReceived;

    /// <summary>
    /// Raised when receive/send loops throw an exception.
    /// </summary>
    public event EventHandler<NightHeavenTCPExceptionEventArgs>? OnException;

    /// <summary>
    /// Unique session identifier for this client connection.
    /// </summary>
    public long SessionId { get; }

    /// <summary>
    /// Receives payload chunk size in bytes.
    /// </summary>
    public int ReceiveBufferSize { get; }

    /// <summary>
    /// Client remote endpoint, when connected.
    /// </summary>
    public EndPoint? RemoteEndPoint
    {
        get
        {
            try
            {
                return _socket.RemoteEndPoint;
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Local endpoint used for this connection, when available.
    /// </summary>
    public EndPoint? LocalEndPoint
    {
        get
        {
            try
            {
                return _socket.LocalEndPoint;
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Gets the number of bytes currently available in the receive circular buffer.
    /// </summary>
    public int AvailableBytes
    {
        get
        {
            lock (_receiveBufferSync)
            {
                return _receiveBuffer.Size;
            }
        }
    }

    /// <summary>
    /// Gets whether the receive circular buffer is full.
    /// </summary>
    public bool IsReceiveBufferFull
    {
        get
        {
            lock (_receiveBufferSync)
            {
                return _receiveBuffer.IsFull;
            }
        }
    }

    /// <summary>
    /// True when the underlying socket is connected and client not closed.
    /// </summary>
    public bool IsConnected => _socket.Connected && Volatile.Read(ref _closed) == 0;

    /// <summary>
    /// Adds a middleware component to this client pipeline.
    /// </summary>
    public NightHeavenTCPClient AddMiddleware(INetMiddleware middleware)
    {
        _middlewarePipeline.AddMiddleware(middleware);

        return this;
    }

    /// <summary>
    /// Closes the client connection and raises disconnect event once.
    /// </summary>
    public async Task CloseAsync()
    {
        if (Interlocked.Exchange(ref _closed, 1) != 0)
        {
            return;
        }

        await _internalCancellationTokenSource.CancelAsync();

        try
        {
            if (_socket.Connected)
            {
                try
                {
                    _socket.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException)
                {
                    // Socket might already be closed by peer.
                }
            }
        }
        finally
        {
            _socket.Close();
            _externalCancellationTokenRegistration.Dispose();
            RaiseDisconnected();
        }
    }

    /// <summary>
    /// Creates an outbound client and connects to the specified endpoint.
    /// </summary>
    public static async Task<NightHeavenTCPClient> ConnectAsync(
        IPEndPoint endPoint,
        IEnumerable<INetMiddleware>? middlewares = null,
        CancellationToken cancellationToken = default
    )
    {
        var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        await socket.ConnectAsync(endPoint, cancellationToken);

        var client = new NightHeavenTCPClient(socket, middlewares);
        await client.StartAsync(cancellationToken);

        return client;
    }

    /// <summary>
    /// Consumes bytes from the front of the receive circular buffer.
    /// </summary>
    public int ConsumeBytes(int count)
    {
        if (count <= 0)
        {
            return 0;
        }

        lock (_receiveBufferSync)
        {
            var bytesToConsume = Math.Min(count, _receiveBuffer.Size);

            for (var i = 0; i < bytesToConsume; i++)
            {
                _receiveBuffer.PopFront();
            }

            return bytesToConsume;
        }
    }

    /// <summary>
    /// Checks whether this client pipeline contains at least one middleware instance of the specified type.
    /// </summary>
    public bool ContainsMiddleware<TMiddleware>()
        where TMiddleware : INetMiddleware
        => _middlewarePipeline.ContainsMiddleware<TMiddleware>();

    /// <summary>
    /// Returns a snapshot of recent received bytes from the circular history buffer.
    /// </summary>
    public byte[] GetRecentReceivedBytes()
        => PeekData();

    /// <summary>
    /// Peeks at data in the receive circular buffer without consuming it.
    /// </summary>
    public byte[] PeekData(int count = 0)
    {
        lock (_receiveBufferSync)
        {
            if (_receiveBuffer.IsEmpty)
            {
                return [];
            }

            var bytesToPeek = count <= 0 ? _receiveBuffer.Size : Math.Min(count, _receiveBuffer.Size);
            var result = new byte[bytesToPeek];

            for (var i = 0; i < bytesToPeek; i++)
            {
                result[i] = _receiveBuffer[i];
            }

            return result;
        }
    }

    /// <summary>
    /// Removes all middleware components of the specified type from this client pipeline.
    /// </summary>
    public bool RemoveMiddleware<TMiddleware>()
        where TMiddleware : INetMiddleware
        => _middlewarePipeline.RemoveMiddleware<TMiddleware>();

    /// <summary>
    /// Sends a payload to the connected socket.
    /// </summary>
    public async Task SendAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        if (payload.IsEmpty || !IsConnected)
        {
            return;
        }

        var processedPayload = await _middlewarePipeline.ExecuteSendAsync(this, payload, cancellationToken);

        if (processedPayload.IsEmpty)
        {
            return;
        }

        await _sendLock.WaitAsync(cancellationToken);

        try
        {
            var sent = 0;

            while (sent < processedPayload.Length && IsConnected)
            {
                var bytesSent = await _socket.SendAsync(processedPayload[sent..], SocketFlags.None, cancellationToken);

                if (bytesSent <= 0)
                {
                    break;
                }

                sent += bytesSent;
            }
        }
        catch (Exception ex)
        {
            RaiseException(ex);
            await CloseAsync();
        }
        finally
        {
            _sendLock.Release();
        }
    }

    /// <summary>
    /// Starts the receive loop and raises connect event.
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _started, 1) != 0)
        {
            return Task.CompletedTask;
        }

        if (cancellationToken.CanBeCanceled)
        {
            _externalCancellationTokenRegistration = cancellationToken.Register(() => _ = CloseAsync());
        }

        RaiseConnected();
        _receiveLoopTask = Task.Run(ReceiveLoopAsync, CancellationToken.None);

        return Task.CompletedTask;
    }

    private void RaiseConnected()
    {
        _logger.Information(
            "Client connected. SessionId={SessionId}, RemoteEndPoint={RemoteEndPoint}",
            SessionId,
            RemoteEndPoint
        );
        OnConnected?.Invoke(this, new(this));
    }

    private void RaiseDisconnected()
    {
        _logger.Information(
            "Client disconnected. SessionId={SessionId}, RemoteEndPoint={RemoteEndPoint}",
            SessionId,
            RemoteEndPoint
        );
        OnDisconnected?.Invoke(this, new(this));
    }

    private void RaiseException(Exception exception)
    {
        _logger.Error(
            exception,
            "Client exception. SessionId={SessionId}, RemoteEndPoint={RemoteEndPoint}",
            SessionId,
            RemoteEndPoint
        );
        OnException?.Invoke(this, new(exception, this));
    }

    private async Task ReceiveLoopAsync()
    {
        var buffer = STArrayPool<byte>.Shared.Rent(ReceiveBufferSize);

        try
        {
            while (!_internalCancellationTokenSource.IsCancellationRequested && IsConnected)
            {
                var received = await _socket.ReceiveAsync(
                                   buffer.AsMemory(0, ReceiveBufferSize),
                                   SocketFlags.None,
                                   _internalCancellationTokenSource.Token
                               );

                if (received <= 0)
                {
                    break;
                }

                var chunk = STArrayPool<byte>.Shared.Rent(received);

                try
                {
                    buffer.AsSpan(0, received).CopyTo(chunk);

                    lock (_receiveBufferSync)
                    {
                        _receiveBuffer.PushBackRange(chunk.AsSpan(0, received));
                    }

                    var chunkMemory = new ReadOnlyMemory<byte>(chunk, 0, received);
                    var processed = await _middlewarePipeline.ExecuteAsync(
                                        this,
                                        chunkMemory,
                                        _internalCancellationTokenSource.Token
                                    );

                    if (!processed.IsEmpty)
                    {
                        OnDataReceived?.Invoke(this, new(this, processed));
                    }
                }
                finally
                {
                    STArrayPool<byte>.Shared.Return(chunk);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during controlled shutdown.
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Receive loop failed for session {SessionId}", SessionId);
            RaiseException(ex);
        }
        finally
        {
            STArrayPool<byte>.Shared.Return(buffer);
            await CloseAsync();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // Sync-over-async: best effort. Prefer DisposeAsync.
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await CloseAsync();

        // Drain the receive loop before disposing the resources it relies on.
        if (_receiveLoopTask is not null)
        {
            try
            {
                await _receiveLoopTask;
            }
            catch
            {
                // Loop failures are already surfaced via OnException.
            }
        }

        _sendLock.Dispose();
        _internalCancellationTokenSource.Dispose();
        _socket.Dispose();
    }
}
