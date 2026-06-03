using DryIoc;
using NightHeaven.Core.Data.Directories;
using NightHeaven.Core.Types;
using NightHeaven.Hosting.Data.Internal;
using NightHeaven.Scripting.Lua.Data.Internal;
using NightHeaven.Server.Extensions.DryIoc;
using NightHeaven.Tests.Plugins.Support;

namespace NightHeaven.Tests.Plugins;

public sealed class PluginContainerExtensionsTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"nh-plugin-container-{Guid.NewGuid():N}");

    [Fact]
    public void AddNightHeavenPlugins_LoadsPluginsBeforeGlobalConfigBinding()
    {
        var directories = new DirectoriesConfig(_root, Enum.GetNames<DirectoryType>());
        PluginFixtureCopy.CopyFixture(directories[DirectoryType.Plugins], "NightHeaven.PluginFixtures.Basic", "basic");
        var container = new Container();
        container.AddNightHeavenLuaScripting(directories);

        container.AddNightHeavenPlugins(directories);
        var sections = container.Resolve<List<ConfigSectionRegistration>>();

        Assert.Contains(sections, section => section.Name == "fixture_plugin");
        Assert.Contains(
            container.Resolve<List<ScriptModuleData>>(),
            module => module.ModuleType.FullName == "NightHeaven.PluginFixtures.Basic.BasicPluginScriptModule"
        );

        var configPath = Path.Combine(directories[DirectoryType.Config], "nightheaven.toml");
        container.AddNightHeavenConfig(configPath);
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
