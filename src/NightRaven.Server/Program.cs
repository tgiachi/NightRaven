using ConsoleAppFramework;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using System.Reflection;
using NightRaven.Core.Data.Directories;
using NightRaven.Core.Types;
using NightRaven.Core.Utils;
using NightRaven.Hosting.Interfaces.Services;
using NightRaven.Hosting.Internal;
using NightRaven.Network.UO.Registry;
using NightRaven.Scripting.Lua.Extensions.Scripts;
using NightRaven.Scripting.Lua.Modules;
using NightRaven.Server.Data;
using NightRaven.Server.Data.Events;
using NightRaven.Server.Extensions;
using NightRaven.Server.Extensions.DryIoc;
using NightRaven.Server.Services.Diagnostics;
using NightRaven.Server.Services.EventBus;
using NightRaven.Server.Services.GameLoop;
using NightRaven.Server.Services.Logging;
using NightRaven.Server.Services.Network;
using NightRaven.Server.Services.Timing;
using Serilog;

await ConsoleApp.RunAsync(
    args,
    (CancellationToken cancellationToken, string? rootDirectory = null, bool debug = false, bool header = true) =>
    {
        rootDirectory = RuntimePaths.ResolveRootDirectory(rootDirectory);

        var directoriesConfig = new DirectoriesConfig(rootDirectory, Enum.GetNames<DirectoryType>());

        if (header)
        {
            var headerContent = ResourceUtils.GetEmbeddedResourceString(
                Assembly.GetExecutingAssembly(),
                "Assets/header.txt"
            );

            Console.WriteLine(headerContent);
        }

        Console.WriteLine($"NightRaven UO Server v{VersionUtils.GetVersion()}");
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
        builder.Services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<NightRavenServiceOrchestrator>());

        // UO packet registry: scan the Network.UO assembly for [PacketHandler] packets.
        var packetRegistry = new PacketRegistry();
        var registeredPackets = PacketTable.Register(packetRegistry);
        Log.Information("Registered {PacketCount} UO packets", registeredPackets);

        // Every NightRaven service registers natively on the DryIoc container (ASP.NET stays on MEDI,
        // which DryIoc imports). This runs after the IServiceCollection descriptors are populated.
        builder.Host.ConfigureContainer<IContainer>(
            container =>
            {
                container.RegisterInstance(packetRegistry);

                // Logger config is loaded with the rest of the TOML sections, then applied immediately.
                container.AddNightRavenLogging();

                // Event bus + game loop (priority 0 / 10) and the diagnostic handler.
                container.AddNightRavenEventBus();
                container.AddTickEventHandler<ServerStartedHandler, ServerStartedEvent>();
                container.AddNightRavenSeeds();

                // Metrics: needs the timer wheel for the background refresh.
                container.AddNightRavenTimerWheel();
                container.AddNightRavenMetrics();
                container.AddMetricProvider<EventBusService>();
                container.AddMetricProvider<GameLoopService>();
                container.AddMetricProvider<TimerWheelService>();

                // UO domain services register persisted entities before persistence starts.
                container.AddNightRavenUsers();
                container.AddDefaultAdminUserSeed();

                // Persistence (priority 15): snapshot + journal.
                container.AddNightRavenPersistence(directoriesConfig[DirectoryType.Save]);

                // Network: TCP game listeners + UDP ping server + packet parser (priority 20).
                container.AddNightRavenNetwork();
                container.AddNightRavenPacketHandlers();
                container.AddMetricProvider<NetworkService>();

                // Lua scripting engine (priority 30).
                container.AddNightRavenLuaScripting(directoriesConfig);

                container.RegisterScriptModule<LogModule>();

                // Plugins can declare config sections, services, Lua modules, persistence entities, and handlers.
                // This must run before AddNightRavenConfig so plugin config sections are bound at boot.
                container.AddNightRavenPlugins(directoriesConfig);

                // Load the root TOML config once and register every section as a DI instance. Must run after
                // all RegisterConfigSection calls (each module helper declares its section).
                container.AddNightRavenConfig(RuntimePaths.ResolveConfigPath(directoriesConfig));
                Log.Logger = LoggerService.CreateLogger(
                    container.Resolve<NightRaven.Hosting.Data.Logging.LoggerConfig>(),
                    directoriesConfig[DirectoryType.Logs]
                );
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
            app.MapNightRavenApiDocs();
        }

        app.UseHttpsRedirection();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.MapNightRavenVersion();
        app.MapNightRavenMetrics();
        app.MapFallbackToFile("index.html");

        app.Run();
    }
);
