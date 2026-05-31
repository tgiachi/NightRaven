using System.Collections.Concurrent;
using NightHeaven.Network.Client;
using NightHeaven.Server.Interfaces.Network;
using NightHeaven.Server.Services.Network.Internal;

namespace NightHeaven.Server.Services.Network;

/// <summary>
/// Thread-safe in-memory <see cref="ISessionService" /> backed by a concurrent dictionary.
/// </summary>
public sealed class SessionService : ISessionService
{
    private readonly ConcurrentDictionary<long, GameSession> _sessions = new();

    public int Count => _sessions.Count;

    public GameSession GetOrCreate(NightHeavenTCPClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        return _sessions.GetOrAdd(client.SessionId, static (_, c) => new GameSession(c), client);
    }

    public bool TryGet(long sessionId, out GameSession session)
        => _sessions.TryGetValue(sessionId, out session!);

    public bool Remove(long sessionId)
        => _sessions.TryRemove(sessionId, out _);

    public void Clear()
        => _sessions.Clear();

    public IReadOnlyCollection<GameSession> GetAll()
        => _sessions.Values.ToArray();
}
