using NightRaven.Abstractions.Data.Network;
using NightRaven.Abstractions.Interfaces.Network;
using NightRaven.Abstractions.Interfaces.Services;
using NightRaven.Network.UO.Interfaces.Packets;

namespace NightRaven.Abstractions.Network;

/// <summary>
/// Base class for typed packet handlers that need common NightRaven services.
/// </summary>
/// <typeparam name="TPacket">Concrete inbound packet type.</typeparam>
public abstract class PacketHandlerBase<TPacket> : IPacketHandler<TPacket>
    where TPacket : IGameNetworkPacket
{
    /// <summary>
    /// Event bus available to packet handlers for publishing domain events.
    /// </summary>
    protected IEventBusService EventBus { get; }

    /// <summary>
    /// Active session manager available to packet handlers.
    /// </summary>
    protected INetworkSessionManager Sessions { get; }

    protected PacketHandlerBase(IEventBusService eventBus, INetworkSessionManager sessions)
    {
        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(sessions);

        EventBus = eventBus;
        Sessions = sessions;
    }

    public abstract Task HandleAsync(
        PacketContext<TPacket> context,
        CancellationToken cancellationToken = default
    );
}
