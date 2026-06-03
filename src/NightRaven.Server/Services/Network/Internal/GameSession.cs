using NightRaven.Network.Client;

namespace NightRaven.Server.Services.Network.Internal;

/// <summary>
/// Minimal per-connection session: tracks the owning client and a pending byte buffer that the
/// network parser accumulates into until full packets can be extracted.
/// </summary>
public sealed class GameSession
{
    private readonly Lock _pendingBytesSync = new();
    private readonly List<byte> _pendingBytes = [];

    public GameSession(NightRavenTCPClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        Client = client;
        SessionId = client.SessionId;
    }

    /// <summary>
    /// Unique identifier of the session, sourced from the owning client.
    /// </summary>
    public long SessionId { get; }

    /// <summary>
    /// Owning TCP client.
    /// </summary>
    public NightRavenTCPClient Client { get; }

    /// <summary>
    /// Executes <paramref name="action" /> with exclusive access to the pending byte buffer.
    /// </summary>
    /// <param name="action">Action receiving the locked pending byte list.</param>
    public void WithPendingBytes(Action<List<byte>> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        lock (_pendingBytesSync)
        {
            action(_pendingBytes);
        }
    }
}
