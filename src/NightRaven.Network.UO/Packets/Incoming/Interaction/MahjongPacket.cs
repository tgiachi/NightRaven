using NightRaven.Network.Spans;
using NightRaven.Network.UO.Attributes;
using NightRaven.Network.UO.Base;
using NightRaven.Network.UO.Types.Packets;

namespace NightRaven.Network.UO.Packets.Incoming.Interaction;

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
