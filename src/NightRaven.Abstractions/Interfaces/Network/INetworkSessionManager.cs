namespace NightRaven.Abstractions.Interfaces.Network;

/// <summary>
/// Read-only view of active network sessions for packet handlers and plugins.
/// </summary>
public interface INetworkSessionManager
{
    /// <summary>
    /// Number of currently tracked sessions.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Returns a snapshot of active session identifiers.
    /// </summary>
    IReadOnlyCollection<long> GetSessionIds();
}
