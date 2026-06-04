using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using NightRaven.Abstractions.Data.Logging;
using NightRaven.Abstractions.Extensions.DryIoc;
using NightRaven.Abstractions.Interfaces.Services;
using NightRaven.Abstractions.Internal;
using NightRaven.Core.Data.Directories;
using NightRaven.Core.Types;
using NightRaven.Network.UO.Registry;
using NightRaven.Server.Bootstrap.Internal;
using NightRaven.Server.Data;
using NightRaven.Server.Data.Events;
using NightRaven.Server.Extensions.Configuration;
using NightRaven.Server.Extensions.Endpoints;
using NightRaven.Server.Extensions.EventBus;
using NightRaven.Server.Extensions.Logging;
using NightRaven.Server.Extensions.Metrics;
using NightRaven.Server.Extensions.Network;
using NightRaven.Server.Extensions.Persistence;
using NightRaven.Server.Extensions.Plugins;
using NightRaven.Server.Extensions.Scripting;
using NightRaven.Server.Extensions.Seed;
using NightRaven.Server.Extensions.Timing;
using NightRaven.Server.Extensions.UoData;
using NightRaven.Server.Extensions.Users;
using NightRaven.Server.Services.Diagnostics;
using NightRaven.Server.Services.EventBus;
using NightRaven.Server.Services.GameLoop;
using NightRaven.Server.Services.Logging;
using NightRaven.Server.Services.Network;
using NightRaven.Server.Services.Timing;
using Serilog;

namespace NightRaven.Server.Bootstrap;

public static class NightRavenBootstrap
{
    public static WebApplication Build(NightRavenBootstrapOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var context = CreateContext(options);
        HeaderPrinter.Print(context, options.ShowHeader);

        var builder = CreateBuilder(options, context);
        var app = builder.Build();
        ConfigurePipeline(app);

        return app;
    }

    public static async Task RunAsync(NightRavenBootstrapOptions options, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        var context = CreateContext(options);
        using var pidFileGuard = PidFileGuard.Acquire(context.Directories);
        HeaderPrinter.Print(context, options.ShowHeader);

        var builder = CreateBuilder(options, context);
        await using var app = builder.Build();
        ConfigurePipeline(app);

        await app.RunAsync(cancellationToken);
    }

    internal static void ConfigureContainer(IContainer container, NightRavenBootstrapContext context)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(context);

        var directories = context.Directories;

        container.RegisterInstance(context.PacketRegistry);

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
        container.AddNightRavenPersistence(directories[DirectoryType.Save]);

        // UO static data: seed bundled reference data, then register client-file + reference stores.
        UoDataAssetsBootstrapper.EnsureDataAssets(
            Path.Combine(AppContext.BaseDirectory, "uo_files"),
            directories[DirectoryType.Data],
            Log.Logger
        );
        container.AddNightRavenUoData(directories[DirectoryType.Data]);

        // Network: TCP game listeners + UDP ping server + packet parser (priority 20).
        container.AddNightRavenNetwork();
        container.AddNightRavenPacketHandlers();
        container.AddMetricProvider<NetworkService>();

        // Lua scripting engine (priority 30).
        container.AddNightRavenLuaScripting(directories);

        // Plugins can declare config sections, services, Lua modules, persistence entities, and handlers.
        // This must run before AddNightRavenConfig so plugin config sections are bound at boot.
        container.AddNightRavenPlugins(directories);

        // Load the root TOML config once and register every section as a DI instance. Must run after
        // all RegisterConfigSection calls (each module helper declares its section).
        container.AddNightRavenConfig(RuntimePaths.ResolveConfigPath(directories));
        Log.Logger = LoggerService.CreateLogger(
            container.Resolve<LoggerConfig>(),
            directories[DirectoryType.Logs]
        );
    }

    internal static WebApplicationBuilder CreateBuilder(
        NightRavenBootstrapOptions options,
        NightRavenBootstrapContext context
    )
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(context);

        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions
            {
                Args = options.Args,
                EnvironmentName = options.Debug ? Environments.Development : null
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

        Log.Information("Registered {PacketCount} UO packets", context.RegisteredPacketCount);

        // Every NightRaven service registers natively on the DryIoc container (ASP.NET stays on MEDI,
        // which DryIoc imports). This runs after the IServiceCollection descriptors are populated.
        builder.Host.ConfigureContainer<IContainer>(container => ConfigureContainer(container, context));

        return builder;
    }

    internal static NightRavenBootstrapContext CreateContext(NightRavenBootstrapOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var rootDirectory = RuntimePaths.ResolveRootDirectory(options.RootDirectory);
        var directories = new DirectoriesConfig(rootDirectory, Enum.GetNames<DirectoryType>());
        var packetRegistry = new PacketRegistry();
        var registeredPacketCount = PacketTable.Register(packetRegistry);

        return new(directories, packetRegistry, registeredPacketCount);
    }

    private static void ConfigurePipeline(WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

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
    }
}
