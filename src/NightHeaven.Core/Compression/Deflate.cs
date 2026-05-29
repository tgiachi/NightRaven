using System.IO.Compression;

namespace NightHeaven.Core.Compression;

public static class Deflate
{
    [ThreadStatic] private static LibDeflateBinding _standard;

    public static LibDeflateBinding Standard => _standard ??= new();
}
