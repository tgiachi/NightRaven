using NightRaven.Network.Spans;
using NightRaven.Network.UO.Attributes;
using NightRaven.Network.UO.Base;
using NightRaven.Network.UO.Types.Packets;

namespace NightRaven.Network.UO.Packets.Incoming.Login;

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
