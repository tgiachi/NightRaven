using NightRaven.Network.Spans;
using NightRaven.Network.UO.Attributes;
using NightRaven.Network.UO.Base;
using NightRaven.Network.UO.Types.Packets;

namespace NightRaven.Network.UO.Packets.Incoming.Interaction;

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
