using NightRaven.Core.Geometry;
using NightRaven.Core.Ids;
using NightRaven.Network.Spans;
using NightRaven.Network.UO.Attributes;
using NightRaven.Network.UO.Base;
using NightRaven.Network.UO.Types.Packets;
using NightRaven.Network.UO.Types.Targeting;

namespace NightRaven.Network.UO.Packets.Incoming.Targeting;

/// <summary>
/// Represents a target cursor command packet.
/// </summary>
[PacketHandler(OpCodeValue, PacketSizing.Fixed, Length = LengthValue, Description = "Target Cursor Commands")]
public class TargetCursorCommandsPacket : BaseGameNetworkPacket
{
    private const byte OpCodeValue = 0x6C;
    private const int LengthValue = 19;

    public TargetCursorSelectionType CursorTarget { get; set; }
    public Serial CursorId { get; set; }
    public TargetCursorType CursorType { get; set; }
    public Serial ClickedOnId { get; set; }
    public Point3D Location { get; set; }
    public byte Unknown { get; set; }
    public ushort Graphic { get; set; }

    public TargetCursorCommandsPacket()
        : base(OpCodeValue, LengthValue) { }

    public TargetCursorCommandsPacket(
        TargetCursorSelectionType cursorTarget,
        Serial cursorId,
        TargetCursorType cursorType
    )
        : this()
    {
        CursorTarget = cursorTarget;
        CursorId = cursorId;
        CursorType = cursorType;
    }

    public static TargetCursorCommandsPacket CreateCancelCurrentTarget()
        => new(TargetCursorSelectionType.SelectObject, (Serial)0u, TargetCursorType.CancelCurrentTargeting);

    public override void Write(ref SpanWriter writer)
    {
        writer.Write(OpCode);
        writer.Write((byte)CursorTarget);
        writer.Write((uint)CursorId);
        writer.Write((byte)CursorType);
        writer.Write((uint)ClickedOnId);
        writer.Write((ushort)Location.X);
        writer.Write((ushort)Location.Y);
        writer.Write(Unknown);
        writer.Write((byte)Location.Z);
        writer.Write(Graphic);
    }

    protected override bool ParsePayload(ref SpanReader reader)
    {
        if (reader.Remaining != 18)
        {
            return false;
        }

        CursorTarget = (TargetCursorSelectionType)reader.ReadByte();
        CursorId = (Serial)reader.ReadUInt32();
        CursorType = (TargetCursorType)reader.ReadByte();
        ClickedOnId = (Serial)reader.ReadUInt32();
        var x = reader.ReadUInt16();
        var y = reader.ReadUInt16();
        Unknown = reader.ReadByte();
        var z = unchecked((sbyte)reader.ReadByte());
        Graphic = reader.ReadUInt16();
        Location = new(x, y, z);

        return reader.Remaining == 0;
    }
}
