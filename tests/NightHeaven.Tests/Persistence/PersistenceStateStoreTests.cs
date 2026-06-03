using NightHeaven.Core.Ids;
using NightHeaven.Persistence.Internal;
using NightHeaven.Tests.Persistence.Support;

namespace NightHeaven.Tests.Persistence;

public class PersistenceStateStoreTests
{
    [Fact]
    public void GetBucket_SameTypeId_ReturnsSameInstance()
    {
        var store = new PersistenceStateStore();

        var a = store.GetBucket<TestPlayer, Serial>(1);
        var b = store.GetBucket<TestPlayer, Serial>(1);

        Assert.Same(a, b);
    }

    [Fact]
    public void GetBucket_DifferentTypeIds_AreIsolated()
    {
        var store = new PersistenceStateStore();

        store.GetBucket<TestPlayer, Serial>(1)[new Serial(7)] = new() { Id = new Serial(7) };

        Assert.Empty(store.GetBucket<TestItem, Serial>(2));
        Assert.Single(store.GetBucket<TestPlayer, Serial>(1));
    }

    [Fact]
    public void ClearBuckets_RemovesAll()
    {
        var store = new PersistenceStateStore();
        store.GetBucket<TestPlayer, Serial>(1)[new Serial(1)] = new() { Id = new Serial(1) };

        store.ClearBuckets();

        Assert.Empty(store.GetBucket<TestPlayer, Serial>(1));
    }
}
