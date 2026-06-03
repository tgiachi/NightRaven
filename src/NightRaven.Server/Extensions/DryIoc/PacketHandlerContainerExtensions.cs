using System.Reflection;
using DryIoc;
using NightRaven.Hosting.Interfaces.Network;
using NightRaven.Network.UO.Interfaces;
using NightRaven.Server.Data.Events;
using NightRaven.Server.Services.Network;

namespace NightRaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for typed packet handlers.
/// </summary>
public static class PacketHandlerContainerExtensions
{
    /// <summary>
    /// Registers the packet dispatcher on the event-bus tick path.
    /// </summary>
    public static IContainer AddNightRavenPacketHandlers(this IContainer container)
    {
        container.AddTickEventHandler<PacketDispatchHandler, PacketReceivedEvent>();

        return container;
    }

    /// <summary>
    /// Registers a typed packet handler.
    /// </summary>
    public static IContainer AddPacketHandler<THandler, TPacket>(this IContainer container)
        where THandler : class, IPacketHandler<TPacket>
        where TPacket : IGameNetworkPacket
    {
        container.Register<THandler>(Reuse.Singleton);
        container.RegisterMapping<IPacketHandler<TPacket>, THandler>();

        return container;
    }

    /// <summary>
    /// Scans an assembly for typed packet handlers and registers each mapping.
    /// </summary>
    public static IContainer AddPacketHandlersFromAssembly(this IContainer container, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        foreach (var handlerType in assembly.GetTypes())
        {
            if (handlerType.IsAbstract || !handlerType.IsClass)
            {
                continue;
            }

            var handlerInterfaces = handlerType.GetInterfaces()
                                               .Where(static interfaceType =>
                                                   interfaceType.IsGenericType &&
                                                   interfaceType.GetGenericTypeDefinition() == typeof(IPacketHandler<>)
                                               )
                                               .ToArray();

            if (handlerInterfaces.Length == 0)
            {
                continue;
            }

            container.Register(handlerType, Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Keep);

            foreach (var handlerInterface in handlerInterfaces)
            {
                container.RegisterMapping(handlerInterface, handlerType);
            }
        }

        return container;
    }
}
