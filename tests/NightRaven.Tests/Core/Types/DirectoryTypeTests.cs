using NightRaven.Core.Types;

namespace NightRaven.Tests.Core.Types;

public class DirectoryTypeTests
{
    [Fact]
    public void DirectoryType_DefinesConfig()
        => Assert.True(Enum.IsDefined(DirectoryType.Config));
}
