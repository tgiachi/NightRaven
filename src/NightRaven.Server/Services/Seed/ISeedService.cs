namespace NightRaven.Server.Services.Seed;

/// <summary>
/// Runs boot-time seed actions once after the server has started.
/// </summary>
public interface ISeedService
{
    /// <summary>
    /// Executes every registered seed action once.
    /// </summary>
    ValueTask RunAsync(CancellationToken cancellationToken = default);
}
