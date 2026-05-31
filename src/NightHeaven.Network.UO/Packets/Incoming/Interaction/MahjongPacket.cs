using NightHeaven.Network.Spans;
using NightHeaven.Network.UO.Attributes;
using NightHeaven.Network.UO.Base;
using NightHeaven.Network.UO.Types.Packets;

namespace NightHeaven.Network.UO.Packets.Incoming.Interaction;

[PacketHandler(0xDA, PacketSizing.Variable, Description = "Mahjong")]

/// <summary>
/// Represents MahjongPacket.
/// </summary>
public class MahjongPacket : BaseGameNetworkPacket
{
    public MahjongPacket()
        : base(0xDA) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
