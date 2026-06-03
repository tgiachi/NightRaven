using System.Collections.Concurrent;
using NightRaven.Network.Client;
using NightRaven.Server.Interfaces.Network;
using NightRaven.Server.Services.Network.Internal;

namespace NightRaven.Server.Services.Network;

/// <summary>
/// Thread-safe in-memory <see cref="ISessionService" /> backed by a concurrent dictionary.
/// </summary>
public sealed class SessionService : ISessionService
{
    private readonly ConcurrentDictionary<long, GameSession> _sessions = new();

    public int Count => _sessions.Count;

    public void Clear()
        => _sessions.Clear();

    public IReadOnlyCollection<GameSession> GetAll()
        => _sessions.Values.ToArray();

    public GameSession GetOrCreate(NightRavenTCPClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        return _sessions.GetOrAdd(client.SessionId, static (_, c) => new(c), client);
    }

    public bool Remove(long sessionId)
        => _sessions.TryRemove(sessionId, out _);

    public bool TryGet(long sessionId, out GameSession session)
        => _sessions.TryGetValue(sessionId, out session!);
}
