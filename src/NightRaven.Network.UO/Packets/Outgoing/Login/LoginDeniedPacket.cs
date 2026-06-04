using NightRaven.Network.Spans;
using NightRaven.Network.UO.Attributes;
using NightRaven.Network.UO.Base;
using NightRaven.Network.UO.Types.Login;
using NightRaven.Network.UO.Types.Packets;

namespace NightRaven.Network.UO.Packets.Outgoing.Login;

/// <summary>
/// Represents a login denied packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Login Denied Response")]
public class LoginDeniedPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x82;
    private const int LengthValue = 2;

    public LoginDeniedReasonType Reason { get; set; }

    public LoginDeniedPacket()
        : base(OpCodeValue, LengthValue) { }

    public LoginDeniedPacket(LoginDeniedReasonType reason)
        : this()
    {
        Reason = reason;
    }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((byte)Reason);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 1)
        {
            return false;
        }

        Reason = (LoginDeniedReasonType)reader.ReadByte();

        return reader.Remaining == 0;
    }
}
