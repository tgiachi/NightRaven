using NightRaven.Core.Ids;

namespace NightRaven.Tests.Persistence.Support;

public sealed class TestItem
{
    public Serial Id { get; set; }
    public string Label { get; set; } = "";
}
