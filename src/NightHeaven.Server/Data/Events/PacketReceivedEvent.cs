using NightHeaven.Hosting.Interfaces.Events;
using NightHeaven.Network.UO.Interfaces;

namespace NightHeaven.Server.Data.Events;

/// <summary>
/// Tick event published for every successfully parsed inbound packet. Handlers run on the
/// game-loop thread via <see cref="ITickEvent" /> dispatch.
/// </summary>
public sealed record PacketReceivedEvent(long SessionId, byte OpCode, IGameNetworkPacket Packet, DateTimeOffset At)
    : ITickEvent;
