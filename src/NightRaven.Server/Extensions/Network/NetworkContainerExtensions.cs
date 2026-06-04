using DryIoc;
using NightRaven.Abstractions.Data.Network;
using NightRaven.Abstractions.Extensions.DryIoc;
using NightRaven.Abstractions.Interfaces.Network;
using NightRaven.Server.Extensions.Hosting;
using NightRaven.Server.Interfaces.Network;
using NightRaven.Server.Services.Network;

namespace NightRaven.Server.Extensions.Network;

/// <summary>
/// DryIoc-native registration helpers for the NightRaven network service.
/// </summary>
public static class NetworkContainerExtensions
{
    private const int NetworkServicePriority = 20;

    /// <summary>
    /// Registers <see cref="NetworkService" /> and <see cref="SessionService" /> with the NightRaven
    /// hosting orchestrator. Requires a <see cref="Network.UO.Registry.PacketRegistry" /> singleton
    /// to have been registered earlier.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    public static IContainer AddNightRavenNetwork(this IContainer container)
    {
        container.AddNightRavenHosting();

        container.RegisterConfigSection("network", () => new NetworkConfig());

        container.Register<SessionService>(Reuse.Singleton);
        container.RegisterMapping<ISessionService, SessionService>();
        container.RegisterMapping<INetworkSessionManager, SessionService>();
        container.Register<IOutgoingPacketQueue, OutgoingPacketQueue>(Reuse.Singleton);
        container.AddNightRavenService<INetworkService, NetworkService>(NetworkServicePriority);

        return container;
    }
}
