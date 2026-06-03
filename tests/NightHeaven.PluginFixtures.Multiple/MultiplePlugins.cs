using DryIoc;
using NightHeaven.Plugins.Data;
using NightHeaven.Plugins.Interfaces;

namespace NightHeaven.PluginFixtures.Multiple;

public sealed class FirstPlugin : INightHeavenPlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "nightheaven.fixture.first",
        Name = "First Fixture Plugin",
        Version = new(1, 0, 0),
        Author = "NightHeaven Tests"
    };

    public void Configure(IContainer container, PluginContext context) { }
}

public sealed class SecondPlugin : INightHeavenPlugin
{
    public PluginMetadata Metadata { get; } = new()
    {
        Id = "nightheaven.fixture.second",
        Name = "Second Fixture Plugin",
        Version = new(1, 0, 0),
        Author = "NightHeaven Tests"
    };

    public void Configure(IContainer container, PluginContext context) { }
}
