using NightRaven.Hosting.Data.Persistence;

namespace NightRaven.Tests.Hosting.Persistence;

public class PersistenceConfigTests
{
    [Fact]
    public void Defaults_MatchExpected()
    {
        var config = new PersistenceConfig();

        Assert.Equal(TimeSpan.FromSeconds(300), config.AutosaveInterval);
        Assert.Equal("world.snapshot.bin", config.SnapshotFileName);
        Assert.Equal("world.journal.bin", config.JournalFileName);
        Assert.True(config.EnableFileLock);
    }
}
