using NightRaven.Network.Spans;
using NightRaven.Network.UO.Attributes;
using NightRaven.Network.UO.Base;
using NightRaven.Network.UO.Types.Packets;

namespace NightRaven.Network.UO.Packets.Incoming.System;

/// <summary>
/// Represents an open UO store packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Open UO Store")]
public class OpenUoStorePacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0xFA;
    private const int LengthValue = 1;

    public OpenUoStorePacket()
        : base(OpCodeValue, LengthValue) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => reader.Remaining == 0;
}
