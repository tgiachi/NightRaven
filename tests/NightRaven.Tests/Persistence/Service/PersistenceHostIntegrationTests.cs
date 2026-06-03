using DryIoc;
using NightRaven.Core.Ids;
using NightRaven.Persistence.Extensions.DryIoc;
using NightRaven.Persistence.Interfaces.Persistence;
using NightRaven.Persistence.Services.Persistence;
using NightRaven.Server.Extensions.DryIoc;
using NightRaven.Tests.Persistence.Support;
using NightRaven.Tests.Support;

namespace NightRaven.Tests.Persistence.Service;

public class PersistenceHostIntegrationTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-persist-host-{Guid.NewGuid():N}");
    private string ConfigPath => Path.Combine(_dir, "nightraven.toml");

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
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
            await players.UpsertAsync(new() { Id = new(1), Name = "Hosted" });

            Assert.Equal("Hosted", (await players.GetByIdAsync(new(1)))!.Name);
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
        var metrics = (PersistenceService)
            container.Resolve<IPersistenceService>();

        var names = metrics.Collect().Select(s => s.Name).ToHashSet();

        Assert.Contains("entities_total", names);
        Assert.Contains("snapshots_written_total", names);
    }

    private IContainer NewContainer()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(ConfigPath, "[persistence]\nenable_file_lock = false\n");

        var container = new Container();
        container.RegisterPersistenceEntity<TestPlayer, Serial>(1, 1, p => p.Id);
        container.RegisterPersistenceEntity<TestItem, Serial>(2, 1, i => i.Id);
        container.AddNightRavenPersistence(_dir);
        container.AddNightRavenConfig(ConfigPath);

        return container;
    }
}
