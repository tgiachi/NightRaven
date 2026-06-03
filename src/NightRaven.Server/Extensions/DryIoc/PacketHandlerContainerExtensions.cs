using DryIoc;
using NightRaven.Abstractions.Extensions.DryIoc;
using NightRaven.Server.Data.Events;
using NightRaven.Server.Services.Network;

namespace NightRaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for the server packet dispatcher.
/// </summary>
public static class PacketHandlerContainerExtensions
{
    extension(IContainer container)
    {
        /// <summary>
        /// Registers the packet dispatcher on the event-bus tick path.
        /// </summary>
        public IContainer AddNightRavenPacketHandlers()
        {
            container.AddTickEventHandler<PacketDispatchHandler, PacketReceivedEvent>();

            return container;
        }
    }
}
