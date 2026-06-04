using DryIoc;
using NightRaven.Server.Extensions.UoData;
using NightRaven.UO.Data.Data;
using NightRaven.UO.Data.Interfaces.Files;

namespace NightRaven.Tests.UO.Data.Hosting;

public class UoDataContainerExtensionsTests
{
    [Fact]
    public void AddNightRavenUoData_RegistersResolverAndVerdataAndConfigSection()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            File.WriteAllBytes(Path.Combine(dir.FullName, "tiledata.mul"), [0]);

            var container = new Container();
            container.RegisterInstance(new UoConfig { ClientFilesDirectory = dir.FullName });
            container.AddNightRavenUoData();

            var resolver = container.Resolve<IUoFileResolver>();
            var verdata = container.Resolve<IVerdataPatchSource>();

            Assert.Equal(dir.FullName, resolver.RootDirectory);
            Assert.Empty(verdata.Patches);
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
