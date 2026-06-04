using NightRaven.Server.Services.UoData;
using NightRaven.Tests.UO.Data.Support;
using NightRaven.UO.Data.Art;
using NightRaven.UO.Data.Bodies;
using NightRaven.UO.Data.Files;
using NightRaven.UO.Data.Hues;
using NightRaven.UO.Data.Localization;
using NightRaven.UO.Data.Maps;
using NightRaven.UO.Data.Multi;
using NightRaven.UO.Data.Races;
using NightRaven.UO.Data.Skills;
using NightRaven.UO.Data.Textures;
using NightRaven.UO.Data.Tiles;

namespace NightRaven.Tests.UO.Data.Hosting;

public class UoDataBootServiceTests
{
    [Fact]
    public async Task StartAsync_EagerLoadsAndLogs_WithoutThrowing()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            TileDataFixture.Write(
                dir.FullName,
                land: [new TileDataFixture.LandEntry(0, 0u, "void")],
                items: []
            );
            var resolver = new UoFileResolver(dir.FullName);

            var service = new UoDataBootService(
                new TileDataStore(resolver),
                new MapService(resolver),
                new LocalizationService(resolver),
                new MultiDataStore(resolver),
                new ArtService(resolver),
                new SkillDataStore(dir.FullName),
                new RaceStore(dir.FullName),
                new BodyDataStore(dir.FullName),
                new HueStore(resolver),
                new RadarColorStore(resolver),
                new TextureStore(resolver)
            );

            await service.StartAsync(CancellationToken.None);
            await service.StopAsync(CancellationToken.None);
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
