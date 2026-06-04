using NightRaven.Network.Spans;
using NightRaven.Network.UO.Attributes;
using NightRaven.Network.UO.Base;
using NightRaven.Network.UO.Types.Environment;
using NightRaven.Network.UO.Types.Packets;

namespace NightRaven.Network.UO.Packets.Outgoing.World;

/// <summary>
/// Represents a season update packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Season")]
public class SeasonPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0xBC;
    private const int LengthValue = 3;

    public bool PlaySound { get; set; }
    public SeasonType Season { get; set; }

    public SeasonPacket()
        : base(OpCodeValue, LengthValue) { }

    public SeasonPacket(SeasonType season, bool playSound = true)
        : this()
    {
        Season = season;
        PlaySound = playSound;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((byte)Season);
        writer.Write(PlaySound);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 2)
        {
            return false;
        }

        Season = (SeasonType)reader.ReadByte();
        PlaySound = reader.ReadBoolean();

        return reader.Remaining == 0;
    }
}
