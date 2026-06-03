using NightHeaven.Network.Spans;
using NightHeaven.Network.UO.Attributes;
using NightHeaven.Network.UO.Base;
using NightHeaven.Network.UO.Types.Packets;

namespace NightHeaven.Network.UO.Packets.Incoming.Login;

[PacketHandler(0x00, PacketSizing.Fixed, Length = 104, Description = "Create Character")]

/// <summary>
/// Represents CreateCharacterPacket.
/// </summary>
public class CreateCharacterPacket : BaseGameNetworkPacket
{
    public CreateCharacterPacket()
        : base(0x00, 104) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
