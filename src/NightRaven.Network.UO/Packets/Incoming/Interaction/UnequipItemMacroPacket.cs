using NightRaven.Network.Spans;
using NightRaven.Network.UO.Attributes;
using NightRaven.Network.UO.Base;
using NightRaven.Network.UO.Types.Packets;

namespace NightRaven.Network.UO.Packets.Incoming.Interaction;

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
