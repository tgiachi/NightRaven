using DryIoc;
using NightHeaven.Hosting.Data.Network;
using NightHeaven.Server.Interfaces.Network;
using NightHeaven.Server.Services.Network;

namespace NightHeaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for the NightHeaven network service.
/// </summary>
public static class NetworkContainerExtensions
{
    private const int NetworkServicePriority = 20;

    /// <summary>
    /// Registers <see cref="NetworkService" /> and <see cref="SessionService" /> with the NightHeaven
    /// hosting orchestrator. Requires a <see cref="Network.UO.Registry.PacketRegistry" /> singleton
    /// to have been registered earlier.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    public static IContainer AddNightHeavenNetwork(this IContainer container)
    {
        container.AddNightHeavenHosting();

        container.RegisterConfigSection("network", () => new NetworkConfig());

        container.Register<ISessionService, SessionService>(Reuse.Singleton);
        container.AddNightHeavenService<INetworkService, NetworkService>(NetworkServicePriority);

        return container;
    }
}
