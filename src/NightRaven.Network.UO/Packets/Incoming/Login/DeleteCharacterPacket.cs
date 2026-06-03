using NightHeaven.Network.Spans;
using NightHeaven.Network.UO.Attributes;
using NightHeaven.Network.UO.Base;
using NightHeaven.Network.UO.Types.Packets;

namespace NightHeaven.Network.UO.Packets.Incoming.Login;

[PacketHandler(0x83, PacketSizing.Fixed, Length = 39, Description = "Delete Character")]

/// <summary>
/// Represents DeleteCharacterPacket.
/// </summary>
public class DeleteCharacterPacket : BaseGameNetworkPacket
{
    public DeleteCharacterPacket()
        : base(0x83, 39) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
