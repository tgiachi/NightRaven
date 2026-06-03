using NightHeaven.Plugins.Data;
using NightHeaven.Plugins.Internal;
using NightHeaven.Tests.Plugins.Support;

namespace NightHeaven.Tests.Plugins;

public class PluginDependencySorterTests
{
    [Fact]
    public void ValidateAndSort_DependentPlugin_ReturnsDependencyFirst()
    {
        var dependent = Loaded("nightheaven.dependent", "nightheaven.dependency");
        var dependency = Loaded("nightheaven.dependency");

        var sorted = PluginDependencySorter.ValidateAndSort([dependent, dependency]);

        Assert.Equal(
            ["nightheaven.dependency", "nightheaven.dependent"],
            sorted.Select(p => p.Metadata.Id).ToArray()
        );
    }

    [Fact]
    public void ValidateAndSort_DuplicateId_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => PluginDependencySorter.ValidateAndSort(
                [Loaded("nightheaven.duplicate"), Loaded("nightheaven.duplicate")]
            )
        );

        Assert.Contains("Duplicate plugin id", ex.Message);
    }

    [Fact]
    public void ValidateAndSort_MissingDependency_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => PluginDependencySorter.ValidateAndSort(
                [Loaded("nightheaven.dependent", "nightheaven.missing")]
            )
        );

        Assert.Contains("missing dependency", ex.Message);
    }

    [Fact]
    public void ValidateAndSort_Cycle_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => PluginDependencySorter.ValidateAndSort(
                [
                    Loaded("nightheaven.a", "nightheaven.b"),
                    Loaded("nightheaven.b", "nightheaven.a")
                ]
            )
        );

        Assert.Contains("cycle", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("")]
    [InlineData("NightHeaven.Bad")]
    [InlineData("nightheaven bad")]
    public void ValidateAndSort_InvalidId_Throws(string id)
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => PluginDependencySorter.ValidateAndSort([Loaded(id)])
        );

        Assert.Contains("plugin id", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static LoadedPlugin Loaded(string id, params string[] dependencies)
        => new(
            Path.Combine(Path.GetTempPath(), $"nh-plugin-{Guid.NewGuid():N}"),
            new FakePlugin(id, dependencies),
            typeof(FakePlugin).Assembly
        );
}
