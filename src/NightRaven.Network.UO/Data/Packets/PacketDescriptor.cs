using NightRaven.Network.UO.Types.Packets;

namespace NightRaven.Network.UO.Data.Packets;

/// <summary>
/// Represents struct.
/// </summary>
public readonly record struct PacketDescriptor(
    byte OpCode,
    PacketSizing Sizing,
    int Length,
    string Description,
    Type HandlerType
);
