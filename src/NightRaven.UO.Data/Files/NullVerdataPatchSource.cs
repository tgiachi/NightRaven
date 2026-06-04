using NightRaven.UO.Data.Data.Internal;
using NightRaven.UO.Data.Interfaces.Files;

namespace NightRaven.UO.Data.Files;

/// <summary>
/// Default <see cref="IVerdataPatchSource" /> that reports no patches. Used by shards that do not
/// ship a <c>verdata.mul</c>.
/// </summary>
public sealed class NullVerdataPatchSource : IVerdataPatchSource
{
    private static readonly VerdataPatch[] _empty = [];

    public IReadOnlyList<VerdataPatch> Patches => _empty;

    public Stream Seek(int lookup)
    {
        throw new InvalidOperationException("No verdata patches are loaded; FileIndex must not request a patched stream.");
    }
}
