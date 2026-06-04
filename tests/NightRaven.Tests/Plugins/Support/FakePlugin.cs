using DryIoc;
using NightRaven.Plugins.Data;
using NightRaven.Plugins.Interfaces.Plugins;

namespace NightRaven.Tests.Plugins.Support;

public sealed class FakePlugin : INightRavenPlugin
{
    public FakePlugin(string id, params string[] dependencies)
    {
        Metadata = new()
        {
            Id = id,
            Name = id,
            Version = new(1, 0, 0),
            Author = "NightRaven Tests",
            Dependencies = dependencies
        };
    }

    public PluginMetadata Metadata { get; }

    public void Configure(IContainer container, PluginContext context) { }
}
