using DryIoc;
using NightHeaven.Plugins.Data;
using NightHeaven.Plugins.Interfaces;

namespace NightHeaven.Tests.Plugins.Support;

public sealed class FakePlugin : INightHeavenPlugin
{
    public FakePlugin(string id, params string[] dependencies)
    {
        Metadata = new()
        {
            Id = id,
            Name = id,
            Version = new(1, 0, 0),
            Author = "NightHeaven Tests",
            Dependencies = dependencies
        };
    }

    public PluginMetadata Metadata { get; }

    public void Configure(IContainer container, PluginContext context) { }
}
