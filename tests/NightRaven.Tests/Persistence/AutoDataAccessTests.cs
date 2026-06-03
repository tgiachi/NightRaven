using NightRaven.Core.Ids;
using NightRaven.Persistence.Data;
using NightRaven.Persistence.Internal;
using NightRaven.Persistence.Services.Persistence;
using NightRaven.Tests.Persistence.Support;

namespace NightRaven.Tests.Persistence;


public class AutoDataAccessTests
{
    // --- Serial key ---

    [Fact]
    public async Task NextId_Serial_FirstCall_ReturnsSerial1()
    {
        var access = NewSerialAccess(out _);

        var id = await access.NextIdAsync();

        Assert.Equal(new Serial(1), id);
    }

    [Fact]
    public async Task NextId_Serial_CalledTwice_ReturnsConsecutiveSerials()
    {
        var access = NewSerialAccess(out _);

        var first = await access.NextIdAsync();
        var second = await access.NextIdAsync();

        Assert.Equal(new Serial(1), first);
        Assert.Equal(new Serial(2), second);
    }

    [Fact]
    public async Task NextId_Serial_AfterUpsert_ContinuesFromMaxId()
    {
        var access = NewSerialAccess(out _);
        await access.UpsertAsync(new() { Id = new(10), Name = "a" });

        var next = await access.NextIdAsync();

        Assert.Equal(new Serial(11), next);
    }

    [Fact]
    public async Task NextId_Serial_AfterRemove_DoesNotReuseId()
    {
        var access = NewSerialAccess(out _);
        await access.UpsertAsync(new() { Id = new(5), Name = "a" });
        await access.RemoveAsync(new Serial(5));

        var next = await access.NextIdAsync();

        Assert.Equal(new Serial(6), next);
    }

    [Fact]
    public async Task NextId_Serial_UpsertWithAllocatedId_StoredCorrectly()
    {
        var access = NewSerialAccess(out _);

        var id = await access.NextIdAsync();
        await access.UpsertAsync(new() { Id = id, Name = "auto" });

        var entity = await access.GetByIdAsync(id);
        Assert.NotNull(entity);
        Assert.Equal("auto", entity!.Name);
    }

    // --- AutoInt32 key ---

    [Fact]
    public async Task NextId_AutoInt32_FirstCall_Returns1()
    {
        var access = NewInt32Access(out _);

        var id = await access.NextIdAsync();

        Assert.Equal(new AutoInt32(1), id);
    }

    [Fact]
    public async Task NextId_AutoInt32_AfterUpsert_ContinuesFromMaxId()
    {
        var access = NewInt32Access(out _);
        await access.UpsertAsync(new() { Id = new AutoInt32(7), Name = "x" });

        var next = await access.NextIdAsync();

        Assert.Equal(new AutoInt32(8), next);
    }

    // --- AutoInt64 key ---

    [Fact]
    public async Task NextId_AutoInt64_FirstCall_Returns1()
    {
        var access = NewInt64Access(out _);

        var id = await access.NextIdAsync();

        Assert.Equal(new AutoInt64(1), id);
    }

    [Fact]
    public async Task NextId_AutoInt64_AfterUpsert_ContinuesFromMaxId()
    {
        var access = NewInt64Access(out _);
        await access.UpsertAsync(new() { Id = new AutoInt64(100), Name = "y" });

        var next = await access.NextIdAsync();

        Assert.Equal(new AutoInt64(101), next);
    }

    private static AutoDataAccess<TestPlayer, Serial> NewSerialAccess(out PersistenceStateStore store)
    {
        store = new();
        var journal = new InMemoryJournalService();
        var descriptor = new PersistenceEntityDescriptor<TestPlayer, Serial>(1, "TestPlayer", 1, p => p.Id);

        return new(store, journal, descriptor);
    }

    private static AutoDataAccess<TestPlayerInt32, AutoInt32> NewInt32Access(out PersistenceStateStore store)
    {
        store = new();
        var journal = new InMemoryJournalService();
        var descriptor = new PersistenceEntityDescriptor<TestPlayerInt32, AutoInt32>(2, "TestPlayerInt32", 1, p => p.Id);

        return new(store, journal, descriptor);
    }

    private static AutoDataAccess<TestPlayerInt64, AutoInt64> NewInt64Access(out PersistenceStateStore store)
    {
        store = new();
        var journal = new InMemoryJournalService();
        var descriptor = new PersistenceEntityDescriptor<TestPlayerInt64, AutoInt64>(3, "TestPlayerInt64", 1, p => p.Id);

        return new(store, journal, descriptor);
    }

}
