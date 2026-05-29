using ConsoleAppFramework;
using NightHeaven.Core.Data.Directories;
using NightHeaven.Core.Types;
using NightHeaven.Core.Utils;
using NightHeaven.Hosting.Interfaces.Services;
using NightHeaven.Server.Data.Events;
using NightHeaven.Server.Extensions;
using NightHeaven.Server.Services.Diagnostics;
using NightHeaven.Server.Services.EventBus;
using NightHeaven.Server.Services.GameLoop;
using NightHeaven.Server.Services.Timing;
using Serilog;

await ConsoleApp.RunAsync(
    args,
    (CancellationToken cancellationToken, string? rootDirectory = null) =>
    {
        rootDirectory ??= Environment.GetEnvironmentVariable("NIGHTHEAVEN_ROOT");

        rootDirectory ??= Path.Combine(Directory.GetCurrentDirectory(), "night_heaven");

        var directoriesConfig = new DirectoriesConfig(rootDirectory, Enum.GetNames<DirectoryType>());

        Console.WriteLine($"NightHeaven UO Server v{VersionUtils.GetVersion()}");
        Console.WriteLine($"Root Directory: {directoriesConfig.Root}");

        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

        builder.Logging.ClearProviders().AddSerilog();

        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // NightHeaven event bus + game loop (priority 0 / 10) and the diagnostic handler.
        builder.Services.AddNightHeavenEventBus();
        builder.Services.AddTickEventHandler<ServerStartedHandler, ServerStartedEvent>();

        // Metrics: needs the timer wheel for the background refresh.
        builder.Services.AddNightHeavenTimerWheel();
        builder.Services.AddNightHeavenMetrics();
        builder.Services.AddMetricProvider<EventBusService>();
        builder.Services.AddMetricProvider<GameLoopService>();
        builder.Services.AddMetricProvider<TimerWheelService>();

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
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.MapNightHeavenMetrics();

        app.Run();
    }
);


