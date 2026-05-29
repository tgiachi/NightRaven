using System.Text;
using NightHeaven.Core.Text;
using NightHeaven.Network.Spans;

namespace NightHeaven.Tests.Network.Spans;

public class SpanReaderTests
{
    [Fact]
    public void ReadByte_AdvancesPosition()
    {
        ReadOnlySpan<byte> data = [0x01, 0x02, 0x03];
        var reader = new SpanReader(data);

        Assert.Equal(0x01, reader.ReadByte());
        Assert.Equal(0x02, reader.ReadByte());
        Assert.Equal(2, reader.Position);
    }

    [Fact]
    public void ReadInt32_BigEndian()
    {
        ReadOnlySpan<byte> data = [0x12, 0x34, 0x56, 0x78];
        var reader = new SpanReader(data);

        Assert.Equal(0x12345678, reader.ReadInt32());
    }

    [Fact]
    public void ReadInt32LE_LittleEndian()
    {
        ReadOnlySpan<byte> data = [0x78, 0x56, 0x34, 0x12];
        var reader = new SpanReader(data);

        Assert.Equal(0x12345678, reader.ReadInt32LE());
    }

    [Fact]
    public void ReadInt64LE_RoundTripsThroughLittleEndian()
    {
        Span<byte> buffer = stackalloc byte[8];
        System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(buffer, 0x1122334455667788L);

        var reader = new SpanReader(buffer);

        Assert.Equal(0x1122334455667788L, reader.ReadInt64LE());
    }

    [Fact]
    public void ReadByte_PastEnd_Throws()
    {
        ReadOnlySpan<byte> data = [];
        var reader = new SpanReader(data);

        // ref struct can't be captured by lambdas; inline check.
        var threw = false;

        try
        {
            reader.ReadByte();
        }
        catch (InvalidOperationException)
        {
            threw = true;
        }

        Assert.True(threw);
    }

    [Fact]
    public void ReadString_WithBclUnicodeEncoding_UsesTwoByteTerminator()
    {
        // "ab\0\0xy" in UTF-16 LE — terminator is two bytes of zeros.
        var bytes = new List<byte>();
        bytes.AddRange(Encoding.Unicode.GetBytes("ab"));
        bytes.Add(0x00);
        bytes.Add(0x00);
        bytes.AddRange(Encoding.Unicode.GetBytes("xy"));

        var reader = new SpanReader(bytes.ToArray());
        var read = reader.ReadString(Encoding.Unicode);

        Assert.Equal("ab", read);
    }

    [Fact]
    public void ReadString_WithCustomNightHeavenUnicodeEncoding_StillUsesTwoByteTerminator()
    {
        // Regression: GetTerminatorWidth used ReferenceEquals against Encoding.Unicode,
        // so NightHeaven's TextEncoding.Unicode (a different UnicodeEncoding instance)
        // was treated as 1-byte terminator and corrupted reads.
        var bytes = new List<byte>();
        bytes.AddRange(TextEncoding.Unicode.GetBytes("ab"));
        bytes.Add(0x00);
        bytes.Add(0x00);
        bytes.AddRange(TextEncoding.Unicode.GetBytes("xy"));

        var reader = new SpanReader(bytes.ToArray());
        var read = reader.ReadString(TextEncoding.Unicode);

        Assert.Equal("ab", read);
    }

    [Fact]
    public void ReadString_AsciiFixedLength_StopsAtFixedSize()
    {
        ReadOnlySpan<byte> data = [(byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e'];
        var reader = new SpanReader(data);

        var read = reader.ReadString(Encoding.ASCII, fixedLength: 3);

        Assert.Equal("abc", read);
        Assert.Equal(3, reader.Position);
    }

    [Fact]
    public void Seek_AbsoluteAndCurrent()
    {
        ReadOnlySpan<byte> data = [1, 2, 3, 4, 5];
        var reader = new SpanReader(data);

        reader.Seek(2, SeekOrigin.Begin);
        Assert.Equal(3, reader.ReadByte());
        reader.Seek(-1, SeekOrigin.Current);
        Assert.Equal(3, reader.ReadByte());
    }
}
