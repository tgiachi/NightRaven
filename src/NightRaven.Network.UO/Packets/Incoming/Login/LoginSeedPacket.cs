using NightRaven.Abstractions.Data.Version;
using NightRaven.Network.Spans;
using NightRaven.Network.UO.Attributes;
using NightRaven.Network.UO.Base;
using NightRaven.Network.UO.Types.Packets;

namespace NightRaven.Network.UO.Packets.Incoming.Login;

[PacketHandler(0xEF, PacketSizing.Fixed, Length = 21, Description = "KR/2D Client Login/Seed")]

/// <summary>
/// Represents LoginSeedPacket.
/// </summary>
public class LoginSeedPacket : BaseGameNetworkPacket
{
    public int Seed { get; set; }
    public ClientVersion ClientVersion { get; set; }

    public LoginSeedPacket()
        : base(0xEF, 21) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        Seed = reader.ReadInt32();
        ClientVersion = new(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());

        return true;
    }
}
