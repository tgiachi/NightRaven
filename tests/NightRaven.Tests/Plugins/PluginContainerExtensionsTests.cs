using DryIoc;
using NightRaven.Abstractions.Data.Internal;
using NightRaven.Core.Data.Directories;
using NightRaven.Core.Types;
using NightRaven.Scripting.Lua.Data.Internal;
using NightRaven.Server.Extensions.DryIoc;
using NightRaven.Tests.Plugins.Support;

namespace NightRaven.Tests.Plugins;

public sealed class PluginContainerExtensionsTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"nh-plugin-container-{Guid.NewGuid():N}");

    [Fact]
    public void AddNightRavenPlugins_LoadsPluginsBeforeGlobalConfigBinding()
    {
        var directories = new DirectoriesConfig(_root, Enum.GetNames<DirectoryType>());
        PluginFixtureCopy.CopyFixture(directories[DirectoryType.Plugins], "NightRaven.PluginFixtures.Basic", "basic");
        var container = new Container();
        container.AddNightRavenLuaScripting(directories);

        container.AddNightRavenPlugins(directories);
        var sections = container.Resolve<List<ConfigSectionRegistration>>();

        Assert.Contains(sections, section => section.Name == "fixture_plugin");
        Assert.Contains(
            container.Resolve<List<ScriptModuleData>>(),
            module => module.ModuleType.FullName == "NightRaven.PluginFixtures.Basic.BasicPluginScriptModule"
        );

        var configPath = Path.Combine(directories[DirectoryType.Config], "nightraven.toml");
        container.AddNightRavenConfig(configPath);
        Assert.Contains("[fixture_plugin]", File.ReadAllText(configPath));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }
}
