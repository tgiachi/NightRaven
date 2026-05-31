using NightHeaven.Network.Spans;
using NightHeaven.Network.UO.Attributes;
using NightHeaven.Network.UO.Base;
using NightHeaven.Network.UO.Types.Packets;

namespace NightHeaven.Network.UO.Packets.Incoming.Interaction;

[PacketHandler(0x95, PacketSizing.Fixed, Length = 9, Description = "Dye Window")]

/// <summary>
/// Represents DyeWindowPacket.
/// </summary>
public class DyeWindowPacket : BaseGameNetworkPacket
{
    public uint TargetSerial { get; set; }

    public ushort Model { get; set; }

    public ushort Hue { get; set; }

    public DyeWindowPacket()
        : base(0x95, 9) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 8)
        {
            return false;
        }

        TargetSerial = reader.ReadUInt32();
        Model = reader.ReadUInt16();
        Hue = reader.ReadUInt16();

        return reader.Remaining == 0;
    }
}
