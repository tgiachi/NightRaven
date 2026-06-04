using NightRaven.Core.Ids;
using NightRaven.Persistence.Internal;
using NightRaven.Persistence.Interfaces.Internal;
using NightRaven.Tests.Persistence.Support;

namespace NightRaven.Tests.Persistence;

public class PersistenceStateStoreTests
{
    [Fact]
    public void ClearBuckets_RemovesAll()
    {
        var store = new PersistenceStateStore();
        store.GetBucket<TestPlayer, Serial>(1)[new(1)] = new() { Id = new(1) };

        store.ClearBuckets();

        Assert.Empty(store.GetBucket<TestPlayer, Serial>(1));
    }

    [Fact]
    public void GetBucket_DifferentTypeIds_AreIsolated()
    {
        var store = new PersistenceStateStore();

        store.GetBucket<TestPlayer, Serial>(1)[new(7)] = new() { Id = new(7) };

        Assert.Empty(store.GetBucket<TestItem, Serial>(2));
        Assert.Single(store.GetBucket<TestPlayer, Serial>(1));
    }

    [Fact]
    public void GetBucket_SameTypeId_ReturnsSameInstance()
    {
        var store = new PersistenceStateStore();

        var a = store.GetBucket<TestPlayer, Serial>(1);
        var b = store.GetBucket<TestPlayer, Serial>(1);

        Assert.Same(a, b);
    }
}
