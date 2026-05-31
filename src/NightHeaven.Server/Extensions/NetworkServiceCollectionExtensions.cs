using NightHeaven.Hosting.Data.Network;
using NightHeaven.Hosting.Extensions;
using NightHeaven.Server.Interfaces.Network;
using NightHeaven.Server.Services.Network;

namespace NightHeaven.Server.Extensions;

/// <summary>
/// DI registration helpers for the NightHeaven network service.
/// </summary>
public static class NetworkServiceCollectionExtensions
{
    private const int NetworkServicePriority = 20;

    /// <summary>
    /// Registers <see cref="NetworkService" /> and <see cref="SessionService" /> with the NightHeaven
    /// hosting orchestrator. Calls <see cref="ServiceCollectionExtensions.AddNightHeavenHosting" />
    /// internally (idempotent). Requires a <see cref="Network.UO.Registry.PacketRegistry" /> singleton
    /// to have been registered earlier.
    /// </summary>
    /// <param name="services">DI service collection.</param>
    /// <param name="configure">Optional callback to customize <see cref="NetworkConfig" />.</param>
    public static IServiceCollection AddNightHeavenNetwork(
        this IServiceCollection services,
        Action<NetworkConfig>? configure = null
    )
    {
        services.AddNightHeavenHosting();

        var config = new NetworkConfig();
        configure?.Invoke(config);
        services.AddSingleton(config);

        services.AddSingleton<ISessionService, SessionService>();
        services.AddNightHeavenService<INetworkService, NetworkService>(NetworkServicePriority);

        return services;
    }
}
