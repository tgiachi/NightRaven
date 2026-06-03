using NightRaven.Network.Client;

namespace NightRaven.Network.Events;

/// <summary>
/// Event payload containing a network client instance.
/// </summary>
public sealed class NightRavenTCPClientEventArgs : EventArgs
{
    public NightRavenTCPClientEventArgs(NightRavenTCPClient client)
    {
        Client = client;
    }

    /// <summary>
    /// Connected or disconnected client.
    /// </summary>
    public NightRavenTCPClient Client { get; }
}
