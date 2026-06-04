using NightRaven.Abstractions.Interfaces.EventHandlers;
using NightRaven.Server.Data.Events;
using NightRaven.Server.Interfaces.Seed;
using Serilog;
using ILogger = Serilog.ILogger;

namespace NightRaven.Server.Services.Seed;

/// <summary>
/// Starts seed execution when the server reports that it has started.
/// </summary>
public sealed class SeedServerStartedHandler : ITickEventHandler<ServerStartedEvent>
{
    private readonly ILogger _logger = Log.ForContext<SeedServerStartedHandler>();
    private readonly ISeedService _seedService;

    public SeedServerStartedHandler(ISeedService seedService)
    {
        _seedService = seedService;
    }

    public void Handle(ServerStartedEvent evt)
        => _ = Task.Run(RunSeedsAsync);

    private async Task RunSeedsAsync()
    {
        try
        {
            await _seedService.RunAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Seed execution failed");
        }
    }
}
