using NightRaven.Network.Spans;
using NightRaven.Network.UO.Attributes;
using NightRaven.Network.UO.Base;
using NightRaven.Network.UO.Types.Packets;

namespace NightRaven.Network.UO.Packets.Incoming.Player;

/// <summary>
/// Represents a ping message packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Ping Message")]
public class PingMessagePacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x73;
    private const int LengthValue = 2;

    public byte Sequence { get; set; }

    public PingMessagePacket()
        : base(OpCodeValue, LengthValue) { }

    public PingMessagePacket(byte sequence)
        : this()
    {
        Sequence = sequence;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write(Sequence);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 1)
        {
            return false;
        }

        Sequence = reader.ReadByte();

        return reader.Remaining == 0;
    }
}
