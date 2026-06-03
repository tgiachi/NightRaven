using NightRaven.Network.Client;

namespace NightRaven.Network.Events;

/// <summary>
/// Event payload containing data received from a network client.
/// </summary>
public sealed class NightRavenTCPDataReceivedEventArgs : EventArgs
{
    public NightRavenTCPDataReceivedEventArgs(NightRavenTCPClient client, ReadOnlyMemory<byte> data)
    {
        Client = client;
        Data = data;
    }

    /// <summary>
    /// Source client for the data payload.
    /// </summary>
    public NightRavenTCPClient Client { get; }

    /// <summary>
    /// Received data payload.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; }
}
