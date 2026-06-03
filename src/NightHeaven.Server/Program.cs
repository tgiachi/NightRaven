using ConsoleAppFramework;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using NightHeaven.Core.Data.Directories;
using NightHeaven.Core.Types;
using NightHeaven.Core.Utils;
using NightHeaven.Hosting.Data.Persistence;
using NightHeaven.Hosting.Interfaces.Services;
using NightHeaven.Hosting.Internal;
using NightHeaven.Network.UO.Registry;
using NightHeaven.Persistence.Services.Persistence;
using NightHeaven.Scripting.Lua.Extensions.Scripts;
using NightHeaven.Scripting.Lua.Modules;
using NightHeaven.Server.Data.Events;
using NightHeaven.Server.Extensions;
using NightHeaven.Server.Extensions.DryIoc;
using NightHeaven.Server.Services.Diagnostics;
using NightHeaven.Server.Services.EventBus;
using NightHeaven.Server.Services.GameLoop;
using NightHeaven.Server.Services.Network;
using NightHeaven.Server.Services.Timing;
using Serilog;

await ConsoleApp.RunAsync(
    args,
    (CancellationToken cancellationToken, string? rootDirectory = null, bool debug = false) =>
    {
        rootDirectory ??= Environment.GetEnvironmentVariable("NIGHTHEAVEN_ROOT");

        rootDirectory ??= Path.Combine(Directory.GetCurrentDirectory(), "night_heaven");

        var directoriesConfig = new DirectoriesConfig(rootDirectory, Enum.GetNames<DirectoryType>());

        Console.WriteLine($"NightHeaven UO Server v{VersionUtils.GetVersion()}");
        Console.WriteLine($"Root Directory: {directoriesConfig.Root}");

        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions
            {
                Args = args,
                EnvironmentName = debug ? Environments.Development : null
            }
        );

        // Back the whole host (REST included) with DryIoc so the Lua scripting engine can
        // register and resolve script-module types at runtime, which MEDI cannot do.
        builder.Host.UseServiceProviderFactory(new DryIocServiceProviderFactory());

        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        builder.Logging.ClearProviders().AddSerilog();

        // ASP.NET Core services (OpenAPI, Kestrel, routing, ...) register through IServiceCollection.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // The generic host collects hosted services from IServiceCollection, so bridge the
        // DryIoc-registered orchestrator here; it resolves its descriptors from the unified provider.
        builder.Services.AddSingleton<IHostedService>(
            sp => sp.GetRequiredService<NightHeavenServiceOrchestrator>()
        );

        // UO packet registry: scan the Network.UO assembly for [PacketHandler] packets.
        var packetRegistry = new PacketRegistry();
        var registeredPackets = PacketTable.Register(packetRegistry);
        Log.Information("Registered {PacketCount} UO packets", registeredPackets);

        // Every NightHeaven service registers natively on the DryIoc container (ASP.NET stays on MEDI,
        // which DryIoc imports). This runs after the IServiceCollection descriptors are populated.
        builder.Host.ConfigureContainer<IContainer>(
            container =>
            {
                container.RegisterInstance(packetRegistry);

                // Event bus + game loop (priority 0 / 10) and the diagnostic handler.
                container.AddNightHeavenEventBus();
                container.AddTickEventHandler<ServerStartedHandler, ServerStartedEvent>();

                // Metrics: needs the timer wheel for the background refresh.
                container.AddNightHeavenTimerWheel();
                container.AddNightHeavenMetrics();
                container.AddMetricProvider<EventBusService>();
                container.AddMetricProvider<GameLoopService>();
                container.AddMetricProvider<TimerWheelService>();


                // Persistence (priority 15): snapshot + journal. No entities registered yet;
                // modules will call RegisterPersistenceEntity<TEntity,TKey>(...) before this runs.
                container.AddNightHeavenPersistence(directoriesConfig[DirectoryType.Save]);

                // Network: TCP game listeners + UDP ping server + packet parser (priority 20).
                container.AddNightHeavenNetwork();
                container.AddMetricProvider<NetworkService>();

                // Lua scripting engine (priority 30).
                container.AddNightHeavenLuaScripting(directoriesConfig);

                container.RegisterScriptModule<LogModule>();

            }
        );

        var app = builder.Build();

        // Publish a tick event the moment the host is up; the handler logs the thread it runs on.
        var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register(
            () =>
            {
                var bus = app.Services.GetRequiredService<IEventBusService>();
                bus.Publish(new ServerStartedEvent(DateTimeOffset.UtcNow));
            }
        );

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapNightHeavenApiDocs();
        }

        app.UseHttpsRedirection();

        app.MapNightHeavenMetrics();

        app.Run();
    }
);


