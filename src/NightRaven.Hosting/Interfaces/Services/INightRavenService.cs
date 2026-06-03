using Microsoft.Extensions.Hosting;

namespace NightRaven.Hosting.Interfaces.Services;

/// <summary>
/// Marker interface for services orchestrated by NightRaven's hosting layer.
/// </summary>
/// <remarks>
/// Extends <see cref="IHostedService" /> so existing .NET hosting conventions
/// (cancellation, async lifecycle, BackgroundService) keep working. The
/// per-service start priority is supplied at registration time via
/// <c>AddNightRavenService</c>, not on the type itself.
/// </remarks>
public interface INightRavenService : IHostedService { }
