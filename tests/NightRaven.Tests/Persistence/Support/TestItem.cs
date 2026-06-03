using NightHeaven.Core.Ids;

namespace NightHeaven.Tests.Persistence.Support;

public sealed class TestItem
{
    public Serial Id { get; set; }
    public string Label { get; set; } = "";
}
