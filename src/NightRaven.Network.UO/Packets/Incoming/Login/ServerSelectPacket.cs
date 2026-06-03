using NightRaven.Network.Spans;
using NightRaven.Network.UO.Attributes;
using NightRaven.Network.UO.Base;
using NightRaven.Network.UO.Types.Packets;

namespace NightRaven.Network.UO.Packets.Incoming.Login;

[PacketHandler(0xA0, PacketSizing.Fixed, Length = 3, Description = "Select Server")]

/// <summary>
/// Represents ServerSelectPacket.
/// </summary>
public class ServerSelectPacket : BaseGameNetworkPacket
{
    public int SelectedServerIndex { get; set; }

    public ServerSelectPacket()
        : base(0xA0, 3) { }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        SelectedServerIndex = reader.ReadInt16();

        return true;
    }
}
