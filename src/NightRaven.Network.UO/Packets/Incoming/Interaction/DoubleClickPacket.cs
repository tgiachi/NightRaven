using NightRaven.Core.Ids;
using NightRaven.Network.Spans;
using NightRaven.Network.UO.Attributes;
using NightRaven.Network.UO.Base;
using NightRaven.Network.UO.Types.Packets;

namespace NightRaven.Network.UO.Packets.Incoming.Interaction;

[PacketHandler(0x06, PacketSizing.Fixed, Length = 5, Description = "Double Click")]

/// <summary>
/// Represents DoubleClickPacket.
/// </summary>
public class DoubleClickPacket : BaseGameNetworkPacket
{
    public Serial TargetSerial { get; set; }

    public DoubleClickPacket()
        : base(0x06, 5) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 4)
        {
            return false;
        }

        TargetSerial = (Serial)reader.ReadUInt32();

        return reader.Remaining == 0;
    }
}
