using NightHeaven.Hosting.Interfaces.Services;

namespace NightHeaven.Server.Interfaces.Network;

/// <summary>
/// Orchestrates the TCP game listeners, the UDP ping server and the inbound packet parser,
/// publishing parsed packets onto the event bus.
/// </summary>
public interface INetworkService : INightHeavenService
{
    /// <summary>
    /// Number of currently connected sessions.
    /// </summary>
    int ConnectedSessionCount { get; }
}
