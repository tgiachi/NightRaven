using NightHeaven.Network.Spans;
using NightHeaven.Network.UO.Attributes;
using NightHeaven.Network.UO.Base;
using NightHeaven.Network.UO.Types.Packets;

namespace NightHeaven.Network.UO.Packets.Incoming.Interaction;

[PacketHandler(0xEC, PacketSizing.Variable, Description = "Equip Macro (KR)")]

/// <summary>
/// Represents EquipMacroPacket.
/// </summary>
public class EquipMacroPacket : BaseGameNetworkPacket
{
    public EquipMacroPacket()
        : base(0xEC) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
