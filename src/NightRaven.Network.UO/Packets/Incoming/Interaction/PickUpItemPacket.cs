using NightHeaven.Core.Ids;
using NightHeaven.Network.Spans;
using NightHeaven.Network.UO.Attributes;
using NightHeaven.Network.UO.Base;
using NightHeaven.Network.UO.Types.Packets;

namespace NightHeaven.Network.UO.Packets.Incoming.Interaction;

[PacketHandler(0x07, PacketSizing.Fixed, Length = 7, Description = "Pick Up Item")]

/// <summary>
/// Represents PickUpItemPacket.
/// </summary>
public class PickUpItemPacket : BaseGameNetworkPacket
{
    public Serial ItemSerial { get; set; }

    public int StackAmount { get; set; }

    public PickUpItemPacket()
        : base(0x07, 7) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 6)
        {
            return false;
        }

        ItemSerial = (Serial)reader.ReadUInt32();
        StackAmount = reader.ReadUInt16();

        return reader.Remaining == 0;
    }
}
