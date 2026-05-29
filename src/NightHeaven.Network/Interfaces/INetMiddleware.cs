using NightHeaven.Network.Client;

namespace NightHeaven.Network.Interfaces;

/// <summary>
/// Defines a middleware component that can inspect and transform
/// incoming network payloads before they are dispatched as events.
/// </summary>
public interface INetMiddleware
{
    /// <summary>
    /// Processes a payload for a specific client.
    /// </summary>
    /// <param name="client">Client associated with the payload, if available.</param>
    /// <param name="data">Incoming payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The transformed payload. Return <see cref="ReadOnlyMemory{T}.Empty" /> to drop the payload.</returns>
    ValueTask<ReadOnlyMemory<byte>> ProcessAsync(
        NightHeavenTCPClient? client,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Processes an outgoing payload before it is sent to the socket.
    /// </summary>
    /// <param name="client">Client associated with the payload, if available.</param>
    /// <param name="data">Outgoing payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The transformed payload. Return <see cref="ReadOnlyMemory{T}.Empty" /> to drop the payload.</returns>
    ValueTask<ReadOnlyMemory<byte>> ProcessSendAsync(
        NightHeavenTCPClient? client,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default
    )
        => ValueTask.FromResult(data);
}
