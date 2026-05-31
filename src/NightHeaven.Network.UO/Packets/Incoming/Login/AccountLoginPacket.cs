using NightHeaven.Network.Spans;
using NightHeaven.Network.UO.Attributes;
using NightHeaven.Network.UO.Base;
using NightHeaven.Network.UO.Types.Packets;

namespace NightHeaven.Network.UO.Packets.Incoming.Login;

[PacketHandler(0x80, PacketSizing.Fixed, Length = 62, Description = "Login Request")]

/// <summary>
/// Represents AccountLoginPacket.
/// </summary>
public class AccountLoginPacket : BaseGameNetworkPacket
{
    public string Account { get; set; }
    public string Password { get; set; }

    public byte NextLoginKey { get; set; }

    public AccountLoginPacket()
        : base(0x80, 62) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        Account = reader.ReadAscii(30);
        Password = reader.ReadAscii(30);
        NextLoginKey = reader.ReadByte();

        return true;
    }
}
