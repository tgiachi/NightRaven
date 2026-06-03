using DryIoc;
using Microsoft.Extensions.DependencyInjection;
using NightRaven.Core.Ids;
using NightRaven.Core.Utils;
using NightRaven.Hosting.Interfaces.EventHandlers;
using NightRaven.Hosting.Interfaces.Services;
using NightRaven.Server.Data.Events;
using NightRaven.Server.Extensions.DryIoc;
using NightRaven.Server.Services.Seed;
using NightRaven.Tests.Support;
using NightRaven.UO.Domain.Interfaces.Services;
using NightRaven.UO.Domain.Types;

namespace NightRaven.Tests.Server.Seed;

public sealed class SeedServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nr-seed-{Guid.NewGuid():N}");
    private string ConfigPath => Path.Combine(_dir, "nightraven.toml");

    [Fact]
    public async Task RunAsync_ExecutesRegisteredActionsOnce()
    {
        var calls = 0;
        var services = new ServiceCollection().BuildServiceProvider();
        var service = new SeedService(
            services,
            [
                (_, _) =>
                {
                    calls++;

                    return ValueTask.CompletedTask;
                }
            ]
        );

        await service.RunAsync();
        await service.RunAsync();

        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task RunAsync_PassesServiceProviderToActions()
    {
        var services = new ServiceCollection()
                       .AddSingleton(new SeedProbe("admin"))
                       .BuildServiceProvider();
        string? captured = null;
        var service = new SeedService(
            services,
            [
                (provider, _) =>
                {
                    captured = provider.GetRequiredService<SeedProbe>().Value;

                    return ValueTask.CompletedTask;
                }
            ]
        );

        await service.RunAsync();

        Assert.Equal("admin", captured);
    }

    [Fact]
    public async Task AddNightRavenSeeds_RegistersHandlerThatRunsSeedsOnServerStartedEvent()
    {
        var calls = 0;
        var container = new Container();
        container.AddNightRavenEventBus();
        container.AddNightRavenSeeds();
        container.AddSeed(
            (_, _) =>
            {
                calls++;

                return ValueTask.CompletedTask;
            }
        );
        container.AddNightRavenConfig(ConfigPath);

        var bus = container.Resolve<IEventBusService>();
        bus.Publish(new ServerStartedEvent(DateTimeOffset.UtcNow));

        Assert.Equal(1, bus.DrainTickEvents(10));
        await WaitUntilAsync(() => Volatile.Read(ref calls) == 1);

        Assert.Equal(1, calls);
        Assert.NotEmpty(container.ResolveMany<ITickEventHandler<ServerStartedEvent>>());
    }

    [Fact]
    public async Task DefaultAdminSeed_CreatesInitialAdminUser()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(ConfigPath, "[persistence]\nenable_file_lock = false\n");

        var container = new Container();
        container.AddNightRavenEventBus();
        container.AddNightRavenSeeds();
        container.AddNightRavenUsers();
        container.AddDefaultAdminUserSeed();
        container.AddNightRavenPersistence(_dir);
        container.AddNightRavenConfig(ConfigPath);

        var orchestrator = container.Orchestrator();
        await orchestrator.StartAsync(CancellationToken.None);

        try
        {
            var bus = container.Resolve<IEventBusService>();
            bus.Publish(new ServerStartedEvent(DateTimeOffset.UtcNow));
            bus.Publish(new ServerStartedEvent(DateTimeOffset.UtcNow));

            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);

            while (DateTime.UtcNow < deadline)
            {
                var users = container.Resolve<IUserService>();

                if (await users.CountAsync() == 1)
                {
                    break;
                }

                await Task.Delay(10);
            }

            var service = container.Resolve<IUserService>();
            var admin = await service.GetByUsernameAsync("admin");

            Assert.NotNull(admin);
            Assert.Equal(new Serial(1), admin!.Id);
            Assert.Equal(UserLevelType.Administrator, admin.Level);
            Assert.True(admin.IsActive);
            Assert.True(HashUtils.VerifyPassword("admin", admin.Password));
            Assert.Equal(1, await service.CountAsync());
        }
        finally
        {
            await orchestrator.StopAsync(CancellationToken.None);
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    private sealed record SeedProbe(string Value);

    private static async Task WaitUntilAsync(Func<bool> predicate)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);

        while (DateTime.UtcNow < deadline)
        {
            if (predicate())
            {
                return;
            }

            await Task.Delay(10);
        }
    }
}
