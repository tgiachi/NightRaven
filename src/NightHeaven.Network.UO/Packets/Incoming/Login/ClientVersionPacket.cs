using NightHeaven.Network.Spans;
using NightHeaven.Network.UO.Attributes;
using NightHeaven.Network.UO.Base;
using NightHeaven.Network.UO.Types.Packets;

namespace NightHeaven.Network.UO.Packets.Incoming.Login;

[PacketHandler(0xBD, PacketSizing.Variable, Description = "Client Version")]

/// <summary>
/// Represents ClientVersionPacket.
/// </summary>
public class ClientVersionPacket : BaseGameNetworkPacket
{
    public string Version { get; set; }

    public ClientVersionPacket()
        : base(0xBD) { }

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((ushort)3);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining < 2)
        {
            return false;
        }

        var length = reader.ReadUInt16();

        if (length < 3)
        {
            return false;
        }

        var payloadLength = length - 3;

        if (payloadLength > reader.Remaining)
        {
            return false;
        }

        Version = payloadLength == 0 ? string.Empty : reader.ReadAscii(payloadLength);

        return true;
    }
}
