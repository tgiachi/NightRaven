using NightRaven.Core.Data.Directories;
using NightRaven.Core.Types;
using NightRaven.Server.Data;

namespace NightRaven.Tests.Server;

public sealed class RuntimePathsTests : IDisposable
{
    private readonly string? _oldPrimary = Environment.GetEnvironmentVariable("NIGHTRAVEN_ROOT");
    private readonly string? _oldLegacy = Environment.GetEnvironmentVariable("NIGHTHEAVEN_ROOT");
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"nr-runtime-paths-{Guid.NewGuid():N}");

    [Fact]
    public void ResolveRootDirectory_CommandLineRoot_WinsOverEnvironment()
    {
        Environment.SetEnvironmentVariable("NIGHTRAVEN_ROOT", Path.Combine(_root, "env"));
        Environment.SetEnvironmentVariable("NIGHTHEAVEN_ROOT", Path.Combine(_root, "legacy"));

        var root = RuntimePaths.ResolveRootDirectory(Path.Combine(_root, "cli"));

        Assert.Equal(Path.Combine(_root, "cli"), root);
    }

    [Fact]
    public void ResolveRootDirectory_PrimaryEnvironment_WinsOverLegacy()
    {
        Environment.SetEnvironmentVariable("NIGHTRAVEN_ROOT", Path.Combine(_root, "new"));
        Environment.SetEnvironmentVariable("NIGHTHEAVEN_ROOT", Path.Combine(_root, "old"));

        var root = RuntimePaths.ResolveRootDirectory(null);

        Assert.Equal(Path.Combine(_root, "new"), root);
    }

    [Fact]
    public void ResolveRootDirectory_LegacyEnvironment_RemainsFallback()
    {
        Environment.SetEnvironmentVariable("NIGHTRAVEN_ROOT", null);
        Environment.SetEnvironmentVariable("NIGHTHEAVEN_ROOT", Path.Combine(_root, "old"));

        var root = RuntimePaths.ResolveRootDirectory(null);

        Assert.Equal(Path.Combine(_root, "old"), root);
    }

    [Fact]
    public void ResolveConfigPath_UsesNewConfigNameByDefault()
    {
        var directories = Directories();

        var configPath = RuntimePaths.ResolveConfigPath(directories);

        Assert.Equal(Path.Combine(directories[DirectoryType.Config], "nightraven.toml"), configPath);
    }

    [Fact]
    public void ResolveConfigPath_UsesLegacyConfigOnlyWhenNewConfigIsMissing()
    {
        var directories = Directories();
        Directory.CreateDirectory(directories[DirectoryType.Config]);
        File.WriteAllText(Path.Combine(directories[DirectoryType.Config], "nightheaven.toml"), string.Empty);

        var configPath = RuntimePaths.ResolveConfigPath(directories);

        Assert.Equal(Path.Combine(directories[DirectoryType.Config], "nightheaven.toml"), configPath);
    }

    [Fact]
    public void ResolveConfigPath_NewConfigWinsOverLegacyConfig()
    {
        var directories = Directories();
        Directory.CreateDirectory(directories[DirectoryType.Config]);
        File.WriteAllText(Path.Combine(directories[DirectoryType.Config], "nightheaven.toml"), string.Empty);
        File.WriteAllText(Path.Combine(directories[DirectoryType.Config], "nightraven.toml"), string.Empty);

        var configPath = RuntimePaths.ResolveConfigPath(directories);

        Assert.Equal(Path.Combine(directories[DirectoryType.Config], "nightraven.toml"), configPath);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("NIGHTRAVEN_ROOT", _oldPrimary);
        Environment.SetEnvironmentVariable("NIGHTHEAVEN_ROOT", _oldLegacy);

        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }

    private DirectoriesConfig Directories()
        => new(_root, Enum.GetNames<DirectoryType>());
}
