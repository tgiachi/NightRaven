using NightHeaven.Network.Client;

namespace NightHeaven.Network.Events;

/// <summary>
/// Event payload containing a network client instance.
/// </summary>
public sealed class NightHeavenTCPClientEventArgs : EventArgs
{
    public NightHeavenTCPClientEventArgs(NightHeavenTCPClient client)
    {
        Client = client;
    }

    /// <summary>
    /// Connected or disconnected client.
    /// </summary>
    public NightHeavenTCPClient Client { get; }
}
