using NightRaven.Core.Ids;

namespace NightRaven.Tests.Persistence.Support;

public sealed class TestPlayer
{
    public Serial Id { get; set; }
    public string Name { get; set; } = "";
    public int Level { get; set; }
}
