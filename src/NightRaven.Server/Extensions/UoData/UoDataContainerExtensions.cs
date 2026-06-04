using DryIoc;
using NightRaven.Abstractions.Extensions.DryIoc;
using NightRaven.Core.Extensions.Directories;
using NightRaven.Server.Extensions.Hosting;
using NightRaven.Server.Services.UoData;
using NightRaven.UO.Data.Art;
using NightRaven.UO.Data.Bodies;
using NightRaven.UO.Data.Data;
using NightRaven.UO.Data.Expansions;
using NightRaven.UO.Data.Files;
using NightRaven.UO.Data.Hues;
using NightRaven.UO.Data.Interfaces.Art;
using NightRaven.UO.Data.Interfaces.Bodies;
using NightRaven.UO.Data.Interfaces.Expansions;
using NightRaven.UO.Data.Interfaces.Hues;
using NightRaven.UO.Data.Interfaces.Files;
using NightRaven.UO.Data.Interfaces.Localization;
using NightRaven.UO.Data.Interfaces.Maps;
using NightRaven.UO.Data.Interfaces.Multi;
using NightRaven.UO.Data.Interfaces.Races;
using NightRaven.UO.Data.Interfaces.Skills;
using NightRaven.UO.Data.Interfaces.Textures;
using NightRaven.UO.Data.Interfaces.Tiles;
using NightRaven.UO.Data.Localization;
using NightRaven.UO.Data.Maps;
using NightRaven.UO.Data.Multi;
using NightRaven.UO.Data.Races;
using NightRaven.UO.Data.Skills;
using NightRaven.UO.Data.Textures;
using NightRaven.UO.Data.Tiles;

namespace NightRaven.Server.Extensions.UoData;

/// <summary>
/// DryIoc-native registration helpers for the NightRaven UO static-data layer.
/// </summary>
public static class UoDataContainerExtensions
{
    private const int UoDataBootPriority = 10;

    /// <summary>
    /// Registers the <c>uo</c> config section, the client-file resolver, the verdata patch source
    /// and the tile-data store.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    public static IContainer AddNightRavenUoData(this IContainer container, string dataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);

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

        container.RegisterDelegate<ISkillDataStore>(_ => new SkillDataStore(dataDirectory), Reuse.Singleton);
        container.RegisterDelegate<IRaceStore>(_ => new RaceStore(dataDirectory), Reuse.Singleton);
        container.RegisterDelegate<IBodyDataStore>(_ => new BodyDataStore(dataDirectory), Reuse.Singleton);
        container.RegisterDelegate<IExpansionStore>(_ => new ExpansionStore(dataDirectory), Reuse.Singleton);

        container.RegisterDelegate<IHueStore>(
            resolver => new HueStore(resolver.Resolve<IUoFileResolver>()),
            Reuse.Singleton
        );
        container.RegisterDelegate<IRadarColorStore>(
            resolver => new RadarColorStore(resolver.Resolve<IUoFileResolver>()),
            Reuse.Singleton
        );
        container.RegisterDelegate<ITextureStore>(
            resolver => new TextureStore(resolver.Resolve<IUoFileResolver>()),
            Reuse.Singleton
        );

        // Eager-load all UO data at boot (priority 10, before the network service at 20).
        container.AddNightRavenHosting();
        container.AddNightRavenService<UoDataBootService>(UoDataBootPriority);

        return container;
    }
}
