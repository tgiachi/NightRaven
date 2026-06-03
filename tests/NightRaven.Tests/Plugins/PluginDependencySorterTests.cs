using NightRaven.Plugins.Data;
using NightRaven.Plugins.Internal;
using NightRaven.Tests.Plugins.Support;

namespace NightRaven.Tests.Plugins;

public class PluginDependencySorterTests
{
    [Fact]
    public void ValidateAndSort_Cycle_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => PluginDependencySorter.ValidateAndSort(
                [
                    Loaded("nightraven.a", "nightraven.b"),
                    Loaded("nightraven.b", "nightraven.a")
                ]
            )
        );

        Assert.Contains("cycle", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateAndSort_DependentPlugin_ReturnsDependencyFirst()
    {
        var dependent = Loaded("nightraven.dependent", "nightraven.dependency");
        var dependency = Loaded("nightraven.dependency");

        var sorted = PluginDependencySorter.ValidateAndSort([dependent, dependency]);

        Assert.Equal(
            ["nightraven.dependency", "nightraven.dependent"],
            sorted.Select(p => p.Metadata.Id).ToArray()
        );
    }

    [Fact]
    public void ValidateAndSort_DuplicateId_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => PluginDependencySorter.ValidateAndSort([Loaded("nightraven.duplicate"), Loaded("nightraven.duplicate")])
        );

        Assert.Contains("Duplicate plugin id", ex.Message);
    }

    [Theory, InlineData(""), InlineData("NightRaven.Bad"), InlineData("nightraven bad")]
    public void ValidateAndSort_InvalidId_Throws(string id)
    {
        var ex = Assert.Throws<InvalidOperationException>(() => PluginDependencySorter.ValidateAndSort([Loaded(id)]));

        Assert.Contains("plugin id", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidateAndSort_MissingDependency_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => PluginDependencySorter.ValidateAndSort([Loaded("nightraven.dependent", "nightraven.missing")])
        );

        Assert.Contains("missing dependency", ex.Message);
    }

    private static LoadedPlugin Loaded(string id, params string[] dependencies)
        => new(
            Path.Combine(Path.GetTempPath(), $"nh-plugin-{Guid.NewGuid():N}"),
            new FakePlugin(id, dependencies),
            typeof(FakePlugin).Assembly
        );
}
