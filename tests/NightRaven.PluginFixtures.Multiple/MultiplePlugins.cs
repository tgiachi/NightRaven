using DryIoc;
using NightRaven.Plugins.Data;
using NightRaven.Plugins.Interfaces.Plugins;

namespace NightRaven.PluginFixtures.Multiple;

public sealed class FirstPlugin : INightRavenPlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "nightraven.fixture.first",
        Name = "First Fixture Plugin",
        Version = new(1, 0, 0),
        Author = "NightRaven Tests"
    };

    public void Configure(IContainer container, PluginContext context) { }
}

public sealed class SecondPlugin : INightRavenPlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "nightraven.fixture.second",
        Name = "Second Fixture Plugin",
        Version = new(1, 0, 0),
        Author = "NightRaven Tests"
    };

    public void Configure(IContainer container, PluginContext context) { }
}
