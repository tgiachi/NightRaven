using NightRaven.Network.Spans;
using NightRaven.Network.UO.Attributes;
using NightRaven.Network.UO.Base;
using NightRaven.Network.UO.Types.Packets;

namespace NightRaven.Network.UO.Packets.Outgoing.World;

/// <summary>
/// Represents a music change packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Set Music")]
public class SetMusicPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x6D;
    private const int LengthValue = 3;

    public int MusicId { get; set; }

    public SetMusicPacket()
        : base(OpCodeValue, LengthValue) { }

    public SetMusicPacket(int musicId)
        : this()
    {
        MusicId = musicId;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((ushort)MusicId);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 2)
        {
            return false;
        }

        MusicId = reader.ReadUInt16();

        return reader.Remaining == 0;
    }
}
