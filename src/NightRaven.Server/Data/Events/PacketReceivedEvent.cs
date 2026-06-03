using NightRaven.Abstractions.Interfaces.Events;
using NightRaven.Network.UO.Interfaces;

namespace NightRaven.Server.Data.Events;

/// <summary>
/// Tick event published for every successfully parsed inbound packet. Handlers run on the
/// game-loop thread via <see cref="ITickEvent" /> dispatch.
/// </summary>
public sealed record PacketReceivedEvent(long SessionId, byte OpCode, IGameNetworkPacket Packet, DateTimeOffset At)
    : ITickEvent;
