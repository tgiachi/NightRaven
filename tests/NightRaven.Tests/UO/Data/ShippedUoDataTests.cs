using NightRaven.UO.Data.Bodies;
using NightRaven.UO.Data.Races;
using NightRaven.UO.Data.Skills;

namespace NightRaven.Tests.UO.Data;

public class ShippedUoDataTests
{
    private static string UoFilesDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        for (var i = 0; i < 8 && dir is not null; i++)
        {
            var candidate = Path.Combine(dir.FullName, "src", "NightRaven.Server", "Assets", "uo_files");

            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate src/NightRaven.Server/uo_files from the test output.");
    }

    [Fact]
    public void ShippedSkills_ParseAndAreComplete()
    {
        var store = new SkillDataStore(UoFilesDirectory());

        Assert.True(store.Count >= 50);
        Assert.NotNull(store.GetByName("Alchemy"));
    }

    [Fact]
    public void ShippedRaces_ParseToThree()
    {
        var store = new RaceStore(UoFilesDirectory());

        Assert.Equal(3, store.Races.Count);
        Assert.Equal("Human", store.GetById(0)!.Name);
    }

    [Fact]
    public void ShippedBodies_ParseToManyEntries()
    {
        var store = new BodyDataStore(UoFilesDirectory());

        Assert.True(store.Count > 1000);
    }

    [Fact]
    public void ShippedExpansions_ParseToTwelve()
    {
        var store = new NightRaven.UO.Data.Expansions.ExpansionStore(UoFilesDirectory());

        Assert.Equal(12, store.Count);
        Assert.Equal("Age of Shadows", store.GetInfo(5)!.Name);
    }
}
