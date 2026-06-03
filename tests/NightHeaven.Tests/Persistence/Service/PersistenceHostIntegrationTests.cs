using global::DryIoc;
using NightHeaven.Core.Ids;
using NightHeaven.Persistence.Interfaces.Persistence;
using NightHeaven.Server.Extensions.DryIoc;
using NightHeaven.Tests.Persistence.Support;
using NightHeaven.Tests.Support;

namespace NightHeaven.Tests.Persistence.Service;

public class PersistenceHostIntegrationTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-persist-host-{Guid.NewGuid():N}");

    private IContainer NewContainer()
    {
        var container = new Container();
        container.RegisterPersistenceEntity<TestPlayer, Serial>(1, 1, p => p.Id);
        container.RegisterPersistenceEntity<TestItem, Serial>(2, 1, i => i.Id);
        container.AddNightHeavenPersistence(_dir, cfg => cfg.EnableFileLock = false);

        return container;
    }

    [Fact]
    public async Task Host_StartsService_AndDataAccessIsInjectable()
    {
        var container = NewContainer();
        var orchestrator = container.Orchestrator();

        await orchestrator.StartAsync(CancellationToken.None);

        try
        {
            var players = container.Resolve<IDataAccess<TestPlayer, Serial>>();
            await players.UpsertAsync(new() { Id = new Serial(1), Name = "Hosted" });

            Assert.Equal("Hosted", (await players.GetByIdAsync(new Serial(1)))!.Name);
        }
        finally
        {
            await orchestrator.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public void PersistenceService_ReportsMetrics()
    {
        var container = NewContainer();
        var metrics = (NightHeaven.Persistence.Services.Persistence.PersistenceService)
            container.Resolve<IPersistenceService>();

        var names = metrics.Collect().Select(s => s.Name).ToHashSet();

        Assert.Contains("entities_total", names);
        Assert.Contains("snapshots_written_total", names);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }
}
