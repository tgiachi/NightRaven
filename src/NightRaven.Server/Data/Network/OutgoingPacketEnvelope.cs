using NightRaven.Network.UO.Interfaces;

namespace NightRaven.Server.Data.Network;

/// <summary>
/// Packet queued for outbound delivery to a connected game session.
/// </summary>
public sealed record OutgoingPacketEnvelope(long SessionId, IGameNetworkPacket Packet, DateTimeOffset EnqueuedAt);
