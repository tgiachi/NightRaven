using DryIoc;
using NightRaven.Abstractions.Extensions.DryIoc;
using NightRaven.Plugins.Data;
using NightRaven.Plugins.Interfaces.Plugins;
using NightRaven.Scripting.Lua.Attributes.Scripts;
using NightRaven.Scripting.Lua.Extensions.Scripts;

namespace NightRaven.PluginFixtures.Basic;

public sealed class BasicPlugin : INightRavenPlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "nightraven.fixture.basic",
        Name = "Basic Fixture Plugin",
        Version = new(1, 0, 0),
        Author = "NightRaven Tests"
    };

    public void Configure(IContainer container, PluginContext context)
    {
        context.LoadConfig(() => new BasicPluginTomlConfig());
        container.RegisterConfigSection("fixture_plugin", () => new BasicPluginServerConfig());
        container.RegisterScriptModule<BasicPluginScriptModule>();
    }
}

public sealed class BasicPluginTomlConfig
{
    public int WeatherIntervalSeconds { get; set; } = 2;
}

public sealed class BasicPluginServerConfig
{
    public string Message { get; set; } = "hello from fixture";
}

[ScriptModule("fixture_basic", "Fixture plugin script module.")]
public sealed class BasicPluginScriptModule;
