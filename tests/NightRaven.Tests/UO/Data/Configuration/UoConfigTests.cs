using NightRaven.UO.Data.Data;

namespace NightRaven.Tests.UO.Data.Configuration;

public class UoConfigTests
{
    [Fact]
    public void Default_ClientFilesDirectory_IsHomeUo()
    {
        var config = new UoConfig();

        Assert.Equal("~/uo", config.ClientFilesDirectory);
    }

    [Fact]
    public void Validate_MissingDirectory_ReturnsError()
    {
        var config = new UoConfig
        {
            ClientFilesDirectory = Path.Combine(Path.GetTempPath(), "nr-uo-does-not-exist-" + Guid.NewGuid().ToString("N"))
        };

        var errors = config.Validate().ToList();

        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Validate_DirectoryWithoutTileData_ReturnsError()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            var config = new UoConfig { ClientFilesDirectory = dir.FullName };

            var errors = config.Validate().ToList();

            Assert.Contains(errors, e => e.Contains("tiledata.mul"));
        }
        finally
        {
            dir.Delete(true);
        }
    }

    [Fact]
    public void Validate_DirectoryWithTileData_IsValid()
    {
        var dir = Directory.CreateTempSubdirectory("nr-uo-");

        try
        {
            File.WriteAllBytes(Path.Combine(dir.FullName, "tiledata.mul"), [0]);
            var config = new UoConfig { ClientFilesDirectory = dir.FullName };

            Assert.Empty(config.Validate());
        }
        finally
        {
            dir.Delete(true);
        }
    }
}
