using NightHeaven.Core.Ids;

namespace NightHeaven.Tests.Persistence.Support;

public sealed class TestPlayer
{
    public Serial Id { get; set; }
    public string Name { get; set; } = "";
    public int Level { get; set; }
}
