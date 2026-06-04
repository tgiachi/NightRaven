using NightRaven.Core.Ids;
using NightRaven.Network.Spans;
using NightRaven.Network.UO.Attributes;
using NightRaven.Network.UO.Base;
using NightRaven.Network.UO.Types.Environment;
using NightRaven.Network.UO.Types.Packets;

namespace NightRaven.Network.UO.Packets.Outgoing.World;

/// <summary>
/// Represents a per-mobile light level packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Personal Light Level")]
public class PersonalLightLevelPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x4E;
    private const int LengthValue = 6;

    public LightLevelType LightLevel { get; set; }
    public Serial MobileSerial { get; set; }

    public PersonalLightLevelPacket()
        : base(OpCodeValue, LengthValue) { }

    public PersonalLightLevelPacket(Serial mobileSerial, LightLevelType lightLevel)
        : this()
    {
        MobileSerial = mobileSerial;
        LightLevel = lightLevel;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((uint)MobileSerial);
        writer.Write((byte)LightLevel);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 5)
        {
            return false;
        }

        MobileSerial = (Serial)reader.ReadUInt32();
        LightLevel = (LightLevelType)reader.ReadByte();

        return reader.Remaining == 0;
    }
}
