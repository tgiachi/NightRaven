using System.Collections.Concurrent;
using NightRaven.Abstractions.Interfaces.Network;
using NightRaven.Abstractions.Interfaces.Services;
using NightRaven.Network.Client;
using NightRaven.Server.Data.Events;
using NightRaven.Server.Interfaces.Network;
using NightRaven.Server.Services.Network.Internal;
using ZLinq;
using ZLinq.Linq;

namespace NightRaven.Server.Services.Network;

/// <summary>
/// Thread-safe in-memory <see cref="ISessionService" /> backed by a concurrent dictionary.
/// </summary>
public sealed class SessionService : ISessionService, INetworkSessionManager
{
    private readonly ConcurrentDictionary<long, GameSession> _sessions = new();
    private readonly IEventBusService? _eventBus;

    public SessionService(IEventBusService? eventBus = null)
    {
        _eventBus = eventBus;
    }

    public int Count => _sessions.Count;

    public void Clear()
        => _sessions.Clear();

    public IReadOnlyCollection<GameSession> GetAll()
        => _sessions.Values.ToArray();

    public GameSession GetOrCreate(NightRavenTCPClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        if (_sessions.TryGetValue(client.SessionId, out var existing))
        {
            return existing;
        }

        var session = new GameSession(client);

        if (!_sessions.TryAdd(client.SessionId, session))
        {
            return _sessions.TryGetValue(client.SessionId, out existing) ? existing : GetOrCreate(client);
        }

        _eventBus?.Publish(
            new PlayerConnectedEvent(session.SessionId, client.RemoteEndPoint?.ToString(), DateTimeOffset.UtcNow)
        );

        return session;
    }

    public IReadOnlyCollection<long> GetSessionIds()
        => _sessions.Keys.ToArray();

    public ValueEnumerable<FromArray<long>, long> QuerySessionIds()
        => _sessions.Keys.ToArray().AsValueEnumerable();

    public bool Remove(long sessionId)
    {
        if (!_sessions.TryRemove(sessionId, out var session))
        {
            return false;
        }

        _eventBus?.Publish(
            new PlayerDisconnectedEvent(session.SessionId, session.Client.RemoteEndPoint?.ToString(), DateTimeOffset.UtcNow)
        );

        return true;
    }

    public bool TryGet(long sessionId, out GameSession session)
        => _sessions.TryGetValue(sessionId, out session!);
}
