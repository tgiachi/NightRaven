using NightHeaven.Core.Types;

namespace NightHeaven.Tests.Core.Types;

public class DirectoryTypeTests
{
    [Fact]
    public void DirectoryType_DefinesConfig()
        => Assert.True(Enum.IsDefined(DirectoryType.Config));
}
