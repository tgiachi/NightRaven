using NightRaven.Network.Spans;
using NightRaven.Network.UO.Attributes;
using NightRaven.Network.UO.Base;
using NightRaven.Network.UO.Types.Packets;

namespace NightRaven.Network.UO.Packets.Incoming.UI;

/// <summary>
/// Represents a help request packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Request Help")]
public class RequestHelpPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x9B;
    private const int LengthValue = 258;

    public byte[] Payload { get; private set; } = [];

    public RequestHelpPacket()
        : base(OpCodeValue, LengthValue) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        Payload = reader.ReadBytes(reader.Remaining);

        return reader.Remaining == 0;
    }
}
