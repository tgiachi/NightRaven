using NightRaven.Network.Spans;
using NightRaven.Network.UO.Attributes;
using NightRaven.Network.UO.Base;
using NightRaven.Network.UO.Types.Packets;

namespace NightRaven.Network.UO.Packets.Incoming.Login;

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
