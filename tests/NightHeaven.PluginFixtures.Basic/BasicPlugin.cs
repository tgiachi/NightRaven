using DryIoc;
using NightHeaven.Plugins.Data;
using NightHeaven.Plugins.Interfaces;
using NightHeaven.Scripting.Lua.Attributes.Scripts;
using NightHeaven.Scripting.Lua.Extensions.Scripts;
using NightHeaven.Server.Extensions.DryIoc;

namespace NightHeaven.PluginFixtures.Basic;

public sealed class BasicPlugin : INightHeavenPlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "nightheaven.fixture.basic",
        Name = "Basic Fixture Plugin",
        Version = new(1, 0, 0),
        Author = "NightHeaven Tests"
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
