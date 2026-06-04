using NightRaven.Core.Data.Directories;
using NightRaven.Network.UO.Registry;

namespace NightRaven.Server.Bootstrap;

public sealed class NightRavenBootstrapContext
{
    public NightRavenBootstrapContext(
        DirectoriesConfig directories,
        PacketRegistry packetRegistry,
        int registeredPacketCount
    )
    {
        Directories = directories;
        PacketRegistry = packetRegistry;
        RegisteredPacketCount = registeredPacketCount;
    }

    public DirectoriesConfig Directories { get; }

    public PacketRegistry PacketRegistry { get; }

    public int RegisteredPacketCount { get; }
}
