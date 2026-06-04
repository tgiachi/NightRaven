using NightRaven.Core.Ids;
using NightRaven.Network.Spans;
using NightRaven.Network.UO.Attributes;
using NightRaven.Network.UO.Base;
using NightRaven.Network.UO.Types.Packets;

namespace NightRaven.Network.UO.Packets.Incoming.Interaction;

[PacketHandler(0x05, PacketSizing.Fixed, Length = 5, Description = "Request Attack")]

/// <summary>
/// Represents RequestAttackPacket.
/// </summary>
public class RequestAttackPacket : BaseGameNetworkPacket
{
    public Serial TargetSerial { get; private set; }

    public RequestAttackPacket()
        : base(0x05, 5) { }

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
