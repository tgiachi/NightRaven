using NightHeaven.Network.Spans;
using NightHeaven.Network.UO.Attributes;
using NightHeaven.Network.UO.Base;
using NightHeaven.Network.UO.Types.Packets;

namespace NightHeaven.Network.UO.Packets.Incoming.Interaction;

[PacketHandler(0xED, PacketSizing.Variable, Description = "Unequip Item Macro (KR)")]

/// <summary>
/// Represents UnequipItemMacroPacket.
/// </summary>
public class UnequipItemMacroPacket : BaseGameNetworkPacket
{
    public UnequipItemMacroPacket()
        : base(0xED) { }

    protected override bool ParsePayload(ref SpanReader reader)
        => true;
}
