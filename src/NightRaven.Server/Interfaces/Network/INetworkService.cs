using NightRaven.Hosting.Interfaces.Services;

namespace NightRaven.Server.Interfaces.Network;

/// <summary>
/// Orchestrates the TCP game listeners, the UDP ping server and the inbound packet parser,
/// publishing parsed packets onto the event bus.
/// </summary>
public interface INetworkService : INightRavenService
{
    /// <summary>
    /// Number of currently connected sessions.
    /// </summary>
    int ConnectedSessionCount { get; }
}
