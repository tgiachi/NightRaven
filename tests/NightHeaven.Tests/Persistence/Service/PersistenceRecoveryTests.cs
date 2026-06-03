using NightHeaven.Core.Ids;
using NightHeaven.Hosting.Data.Persistence;
using NightHeaven.Persistence.Data;
using NightHeaven.Persistence.Services.Persistence;
using NightHeaven.Tests.Persistence.Support;

namespace NightHeaven.Tests.Persistence.Service;

public class PersistenceRecoveryTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-persist-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Recovery_FromJournalOnly_RebuildsBothEntityTypes()
    {
        var itemId = new Serial(Serial.ItemOffset + 1);

        var first = NewService();
        await first.StartAsync(CancellationToken.None);
        await first.GetDataAccess<TestPlayer, Serial>().UpsertAsync(new() { Id = new(1), Name = "Bob" });
        await first.GetDataAccess<TestItem, Serial>().UpsertAsync(new() { Id = itemId, Label = "Sword" });
        await first.StopWithoutSnapshotAsync();

        var second = NewService();
        await second.StartAsync(CancellationToken.None);

        Assert.Equal(1, await second.GetDataAccess<TestPlayer, Serial>().CountAsync());
        Assert.Equal("Bob", (await second.GetDataAccess<TestPlayer, Serial>().GetByIdAsync(new(1)))!.Name);
        Assert.Equal("Sword", (await second.GetDataAccess<TestItem, Serial>().GetByIdAsync(itemId))!.Label);
        await second.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Recovery_FromSnapshot_RebuildsStateAndTrimsJournal()
    {
        var first = NewService();
        await first.StartAsync(CancellationToken.None);
        await first.GetDataAccess<TestPlayer, Serial>().UpsertAsync(new() { Id = new(5), Name = "Snap" });
        await first.SaveSnapshotAsync();
        await first.StopAsync(CancellationToken.None);

        var second = NewService();
        await second.StartAsync(CancellationToken.None);

        Assert.Equal("Snap", (await second.GetDataAccess<TestPlayer, Serial>().GetByIdAsync(new(5)))!.Name);
        await second.StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Recovery_SnapshotPlusNewerJournal_AppliesBoth()
    {
        var first = NewService();
        await first.StartAsync(CancellationToken.None);
        var players = first.GetDataAccess<TestPlayer, Serial>();
        await players.UpsertAsync(new() { Id = new(1), Name = "InSnapshot" });
        await first.SaveSnapshotAsync();
        await players.UpsertAsync(new() { Id = new(2), Name = "AfterSnapshot" });
        await first.StopWithoutSnapshotAsync();

        var second = NewService();
        await second.StartAsync(CancellationToken.None);

        Assert.Equal(2, await second.GetDataAccess<TestPlayer, Serial>().CountAsync());
        Assert.Equal("AfterSnapshot", (await second.GetDataAccess<TestPlayer, Serial>().GetByIdAsync(new(2)))!.Name);
        await second.StopAsync(CancellationToken.None);
    }

    private PersistenceService NewService()
    {
        var config = new PersistenceConfig { EnableFileLock = false };
        var registrations = new List<PersistenceEntityRegistration>
        {
            new(new PersistenceEntityDescriptor<TestPlayer, Serial>(1, "TestPlayer", 1, p => p.Id)),
            new(new PersistenceEntityDescriptor<TestItem, Serial>(2, "TestItem", 1, i => i.Id))
        };

        return new(_dir, config, registrations);
    }
}
