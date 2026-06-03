using NightHeaven.Network.UO.Data.Packets;
using NightHeaven.Network.UO.Interfaces;

namespace NightHeaven.Network.UO.Data.Internal.Packets;

internal readonly record struct PacketRegistration(
    PacketDescriptor Descriptor,
    Func<IGameNetworkPacket> Factory
);
