using ConsoleAppFramework;
using NightHeaven.Core.Data.Directories;
using NightHeaven.Core.Types;
using NightHeaven.Core.Utils;
using NightHeaven.Hosting.Interfaces.Metrics;
using NightHeaven.Hosting.Interfaces.Services;
using NightHeaven.Server.Data.Events;
using NightHeaven.Server.Extensions;
using NightHeaven.Server.Services.Diagnostics;
using NightHeaven.Server.Services.Metrics;
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
        builder.Services.AddMetricProvider<NightHeaven.Server.Services.EventBus.EventBusService>();
        builder.Services.AddMetricProvider<NightHeaven.Server.Services.GameLoop.GameLoopService>();
        builder.Services.AddMetricProvider<NightHeaven.Server.Services.Timing.TimerWheelService>();

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

        app.MapGet(
               "/metrics",
               (IMetricsService metrics) => Results.Text(
                   OpenMetricsFormatter.Format(metrics.GetSnapshot()),
                   contentType: "application/openmetrics-text; version=1.0.0; charset=utf-8"
               )
           )
           .WithName("GetMetrics");

        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        app.MapGet(
               "/weatherforecast",
               () =>
               {
                   var forecast = Enumerable.Range(1, 5)
                                            .Select(
                                                index =>
                                                    new WeatherForecast(
                                                        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                                                        Random.Shared.Next(-20, 55),
                                                        summaries[Random.Shared.Next(summaries.Length)]
                                                    )
                                            )
                                            .ToArray();

                   return forecast;
               }
           )
           .WithName("GetWeatherForecast");

        app.Run();
    }
);

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
