using DryIoc;
using NightRaven.Abstractions.Extensions.DryIoc;
using NightRaven.Core.Extensions.Directories;
using NightRaven.UO.Data.Art;
using NightRaven.UO.Data.Data;
using NightRaven.UO.Data.Files;
using NightRaven.UO.Data.Interfaces.Art;
using NightRaven.UO.Data.Interfaces.Files;
using NightRaven.UO.Data.Interfaces.Localization;
using NightRaven.UO.Data.Interfaces.Maps;
using NightRaven.UO.Data.Interfaces.Multi;
using NightRaven.UO.Data.Interfaces.Tiles;
using NightRaven.UO.Data.Localization;
using NightRaven.UO.Data.Maps;
using NightRaven.UO.Data.Multi;
using NightRaven.UO.Data.Tiles;

namespace NightRaven.Server.Extensions.UoData;

/// <summary>
/// DryIoc-native registration helpers for the NightRaven UO static-data layer.
/// </summary>
public static class UoDataContainerExtensions
{
    /// <summary>
    /// Registers the <c>uo</c> config section, the client-file resolver, the verdata patch source
    /// and the tile-data store.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    public static IContainer AddNightRavenUoData(this IContainer container)
    {
        container.RegisterConfigSection("uo", () => new UoConfig());

        container.RegisterDelegate<IUoFileResolver>(
            resolver => new UoFileResolver(resolver.Resolve<UoConfig>().ClientFilesDirectory.ResolvePathAndEnvs()),
            Reuse.Singleton
        );

        container.Register<IVerdataPatchSource, NullVerdataPatchSource>(Reuse.Singleton);

        container.RegisterDelegate<ITileDataStore>(
            resolver => new TileDataStore(resolver.Resolve<IUoFileResolver>()),
            Reuse.Singleton
        );

        container.RegisterDelegate<IMapService>(
            resolver => new MapService(resolver.Resolve<IUoFileResolver>()),
            Reuse.Singleton
        );

        container.RegisterDelegate<ILocalizationService>(
            resolver => new LocalizationService(resolver.Resolve<IUoFileResolver>()),
            Reuse.Singleton
        );

        container.RegisterDelegate<IMultiDataStore>(
            resolver => new MultiDataStore(resolver.Resolve<IUoFileResolver>()),
            Reuse.Singleton
        );

        container.RegisterDelegate<IArtService>(
            resolver => new ArtService(resolver.Resolve<IUoFileResolver>()),
            Reuse.Singleton
        );

        return container;
    }
}
