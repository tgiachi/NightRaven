using NightRaven.Network.UO.Data.Packets;
using NightRaven.Network.UO.Interfaces;

namespace NightRaven.Network.UO.Data.Internal.Packets;

internal readonly record struct PacketRegistration(
    PacketDescriptor Descriptor,
    Func<IGameNetworkPacket> Factory
);
