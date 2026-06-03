using NightHeaven.Network.Client;

namespace NightHeaven.Network.Events;

/// <summary>
/// Event payload containing data received from a network client.
/// </summary>
public sealed class NightHeavenTCPDataReceivedEventArgs : EventArgs
{
    public NightHeavenTCPDataReceivedEventArgs(NightHeavenTCPClient client, ReadOnlyMemory<byte> data)
    {
        Client = client;
        Data = data;
    }

    /// <summary>
    /// Source client for the data payload.
    /// </summary>
    public NightHeavenTCPClient Client { get; }

    /// <summary>
    /// Received data payload.
    /// </summary>
    public ReadOnlyMemory<byte> Data { get; }
}
