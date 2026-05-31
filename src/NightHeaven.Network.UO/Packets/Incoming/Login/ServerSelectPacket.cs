using NightHeaven.Network.Spans;
using NightHeaven.Network.UO.Attributes;
using NightHeaven.Network.UO.Base;
using NightHeaven.Network.UO.Types.Packets;

namespace NightHeaven.Network.UO.Packets.Incoming.Login;

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
