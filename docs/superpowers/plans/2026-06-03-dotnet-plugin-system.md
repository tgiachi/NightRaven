# .NET Plugin System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a boot-time .NET plugin loader that scans `plugins/`, validates plugin metadata/dependencies, loads plugin DLLs, and lets plugins register NightHeaven container features before global TOML config binding.

**Architecture:** Add a new `NightHeaven.Plugins` project containing the public plugin contract, plugin context/config loading, dependency ordering, assembly loading, and `PluginLoaderService`. The Server project exposes a DryIoc helper `AddNightHeavenPlugins(directoriesConfig)` and calls it in `Program.cs` after built-in modules and before `AddNightHeavenConfig(...)`. Tests use small fixture plugin projects copied into temporary plugin folders so assembly loading is exercised for real.

**Tech Stack:** .NET 10, DryIoc, Tomlyn 2.4.1, Serilog, AssemblyLoadContext/AssemblyDependencyResolver, xUnit.

---

## File Structure

**New project: `src/NightHeaven.Plugins/`**

- `NightHeaven.Plugins.csproj` - public plugin API and loader library. References Core, Hosting, DryIoc, Tomlyn, Serilog.
- `Interfaces/INightHeavenPlugin.cs` - public plugin contract.
- `Data/PluginMetadata.cs` - public metadata record/class exposed by plugins.
- `Data/PluginContext.cs` - public per-plugin context with TOML config helper.
- `Data/LoadedPlugin.cs` - loaded plugin descriptor returned by the loader.
- `Internal/PluginAssemblyLoadContext.cs` - per-plugin non-collectible assembly load context.
- `Internal/PluginDependencySorter.cs` - validates metadata, duplicate IDs, missing dependencies, cycles, and returns dependency order.
- `Services/PluginLoaderService.cs` - scans plugin directories, loads assemblies, instantiates plugins, sorts them, and calls `Configure`.

**Server integration**

- `src/NightHeaven.Server/NightHeaven.Server.csproj` - reference `NightHeaven.Plugins`.
- `src/NightHeaven.Server/Extensions/DryIoc/PluginContainerExtensions.cs` - `AddNightHeavenPlugins`.
- `src/NightHeaven.Server/Program.cs` - call `AddNightHeavenPlugins(directoriesConfig)` before `AddNightHeavenConfig(...)`.

**Solution/test project**

- `NightHeaven.slnx` - add `NightHeaven.Plugins` and plugin fixture projects.
- `tests/NightHeaven.Tests/NightHeaven.Tests.csproj` - reference `NightHeaven.Plugins` and fixture projects.

**Test fixture projects**

- `tests/NightHeaven.PluginFixtures.Basic/` - one valid plugin that registers a config section and Lua script module.
- `tests/NightHeaven.PluginFixtures.Empty/` - no plugin implementation, used for load failure tests.
- `tests/NightHeaven.PluginFixtures.Multiple/` - two plugin implementations, used for load failure tests.

**Tests**

- `tests/NightHeaven.Tests/Plugins/PluginContextTests.cs`
- `tests/NightHeaven.Tests/Plugins/PluginDependencySorterTests.cs`
- `tests/NightHeaven.Tests/Plugins/PluginLoaderServiceTests.cs`
- `tests/NightHeaven.Tests/Plugins/PluginContainerExtensionsTests.cs`
- `tests/NightHeaven.Tests/Plugins/Support/FakePlugin.cs`
- `tests/NightHeaven.Tests/Plugins/Support/PluginFixtureCopy.cs`

## Task 1: Add `NightHeaven.Plugins` project and public contracts

**Files:**

- Modify: `NightHeaven.slnx`
- Create: `src/NightHeaven.Plugins/NightHeaven.Plugins.csproj`
- Create: `src/NightHeaven.Plugins/Interfaces/INightHeavenPlugin.cs`
- Create: `src/NightHeaven.Plugins/Data/PluginMetadata.cs`

- [ ] **Step 1: Add the project file**

Create `src/NightHeaven.Plugins/NightHeaven.Plugins.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="DryIoc.dll" Version="5.4.3"/>
        <PackageReference Include="Serilog" Version="4.3.1" />
        <PackageReference Include="Tomlyn" Version="2.4.1" />
    </ItemGroup>

    <ItemGroup>
        <ProjectReference Include="..\NightHeaven.Core\NightHeaven.Core.csproj" />
        <ProjectReference Include="..\NightHeaven.Hosting\NightHeaven.Hosting.csproj" />
    </ItemGroup>

    <ItemGroup>
        <InternalsVisibleTo Include="NightHeaven.Tests" />
    </ItemGroup>

</Project>
```

- [ ] **Step 2: Add the project to the solution**

Modify `NightHeaven.slnx` so it includes the plugin project:

```xml
<Solution>
    <Project Path="src/NightHeaven.Scripting.Lua/NightHeaven.Scripting.Lua.csproj" />
    <Project Path="src/NightHeaven.Core/NightHeaven.Core.csproj" />
    <Project Path="src/NightHeaven.Persistence/NightHeaven.Persistence.csproj" />
    <Project Path="src/NightHeaven.Hosting/NightHeaven.Hosting.csproj" />
    <Project Path="src/NightHeaven.Plugins/NightHeaven.Plugins.csproj" />
    <Project Path="src/NightHeaven.Network.UO/NightHeaven.Network.UO.csproj" />
    <Project Path="src/NightHeaven.Network/NightHeaven.Network.csproj" />
    <Project Path="src/NightHeaven.Server/NightHeaven.Server.csproj" />
    <Project Path="tests/NightHeaven.Tests/NightHeaven.Tests.csproj" />
</Solution>
```

- [ ] **Step 3: Create metadata type**

Create `src/NightHeaven.Plugins/Data/PluginMetadata.cs`:

```csharp
namespace NightHeaven.Plugins.Data;

/// <summary>
/// Describes a NightHeaven plugin. This is the source of truth for plugin identity.
/// </summary>
public sealed class PluginMetadata
{
    /// <summary>Stable lowercase dotted plugin identifier, for example <c>nightheaven.weather</c>.</summary>
    public required string Id { get; init; }

    /// <summary>Human-readable plugin name.</summary>
    public required string Name { get; init; }

    /// <summary>Plugin version.</summary>
    public required Version Version { get; init; }

    /// <summary>Plugin author.</summary>
    public required string Author { get; init; }

    /// <summary>Optional human-readable description.</summary>
    public string? Description { get; init; }

    /// <summary>Plugin IDs that must load before this plugin.</summary>
    public IReadOnlyList<string> Dependencies { get; init; } = [];
}
```

- [ ] **Step 4: Create plugin interface**

Create `src/NightHeaven.Plugins/Interfaces/INightHeavenPlugin.cs`:

```csharp
using DryIoc;
using NightHeaven.Plugins.Data;

namespace NightHeaven.Plugins.Interfaces;

/// <summary>
/// Implemented by trusted .NET plugins loaded by NightHeaven during server startup.
/// </summary>
public interface INightHeavenPlugin
{
    /// <summary>Plugin identity, descriptive information, and dependency declarations.</summary>
    PluginMetadata Metadata { get; }

    /// <summary>
    /// Registers the plugin's services, handlers, config sections, Lua modules, and other integrations.
    /// Called during container configuration before global server TOML config is loaded.
    /// </summary>
    /// <param name="container">The DryIoc container being configured.</param>
    /// <param name="context">The plugin-specific boot context.</param>
    void Configure(IContainer container, PluginContext context);
}
```

This will not compile yet because `PluginContext` is added in Task 2.

- [ ] **Step 5: Run build to verify the expected failure**

Run:

```bash
dotnet build src/NightHeaven.Plugins/NightHeaven.Plugins.csproj -c Debug -nologo
```

Expected: FAIL with a compiler error mentioning `PluginContext` is not defined.

Do not commit yet. Task 2 completes the public contract.

## Task 2: Add plugin TOML config context

**Files:**

- Create: `src/NightHeaven.Plugins/Data/PluginContext.cs`
- Test: `tests/NightHeaven.Tests/Plugins/PluginContextTests.cs`
- Modify: `tests/NightHeaven.Tests/NightHeaven.Tests.csproj`

- [ ] **Step 1: Reference `NightHeaven.Plugins` from tests**

Modify `tests/NightHeaven.Tests/NightHeaven.Tests.csproj` and add this project reference:

```xml
<ProjectReference Include="..\..\src\NightHeaven.Plugins\NightHeaven.Plugins.csproj"/>
```

The references block should include:

```xml
<ItemGroup>
    <ProjectReference Include="..\..\src\NightHeaven.Core\NightHeaven.Core.csproj"/>
    <ProjectReference Include="..\..\src\NightHeaven.Hosting\NightHeaven.Hosting.csproj"/>
    <ProjectReference Include="..\..\src\NightHeaven.Network\NightHeaven.Network.csproj"/>
    <ProjectReference Include="..\..\src\NightHeaven.Persistence\NightHeaven.Persistence.csproj"/>
    <ProjectReference Include="..\..\src\NightHeaven.Plugins\NightHeaven.Plugins.csproj"/>
    <ProjectReference Include="..\..\src\NightHeaven.Server\NightHeaven.Server.csproj"/>
</ItemGroup>
```

- [ ] **Step 2: Write failing tests**

Create `tests/NightHeaven.Tests/Plugins/PluginContextTests.cs`:

```csharp
using NightHeaven.Core.Data.Directories;
using NightHeaven.Core.Types;
using NightHeaven.Plugins.Data;

namespace NightHeaven.Tests.Plugins;

public sealed class PluginContextTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        $"nh-plugin-context-{Guid.NewGuid():N}"
    );

    private string PluginDirectory => Path.Combine(_root, "plugins", "nightheaven.test");

    [Fact]
    public void LoadConfig_MissingFile_WritesDefaultsAndReturnsDefaults()
    {
        var context = CreateContext();

        var config = context.LoadConfig(() => new WeatherPluginConfig());

        Assert.Equal(2, config.WeatherIntervalSeconds);
        Assert.True(File.Exists(context.PluginConfigPath));
        Assert.Contains("weather_interval_seconds = 2", File.ReadAllText(context.PluginConfigPath));
    }

    [Fact]
    public void LoadConfig_ExistingFile_BindsValues()
    {
        Directory.CreateDirectory(PluginDirectory);
        File.WriteAllText(
            Path.Combine(PluginDirectory, "plugin.toml"),
            "weather_interval_seconds = 7\nregion = \"Trammel\"\n"
        );
        var context = CreateContext();

        var config = context.LoadConfig(() => new WeatherPluginConfig());

        Assert.Equal(7, config.WeatherIntervalSeconds);
        Assert.Equal("Trammel", config.Region);
    }

    [Fact]
    public void LoadConfig_MalformedFile_Throws()
    {
        Directory.CreateDirectory(PluginDirectory);
        File.WriteAllText(Path.Combine(PluginDirectory, "plugin.toml"), "weather_interval_seconds = = =\n");
        var context = CreateContext();

        var ex = Assert.Throws<InvalidOperationException>(
            () => context.LoadConfig(() => new WeatherPluginConfig())
        );
        Assert.Contains("plugin.toml", ex.Message);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }

    private PluginContext CreateContext()
        => new(PluginDirectory, new DirectoriesConfig(_root, Enum.GetNames<DirectoryType>()));

    private sealed class WeatherPluginConfig
    {
        public int WeatherIntervalSeconds { get; set; } = 2;
        public string Region { get; set; } = "Britannia";
    }
}
```

- [ ] **Step 3: Run tests to verify failure**

Run:

```bash
dotnet test tests/NightHeaven.Tests/NightHeaven.Tests.csproj --filter "FullyQualifiedName~PluginContextTests" --nologo
```

Expected: FAIL because `PluginContext` does not exist.

- [ ] **Step 4: Implement `PluginContext`**

Create `src/NightHeaven.Plugins/Data/PluginContext.cs`:

```csharp
using NightHeaven.Core.Data.Directories;
using NightHeaven.Hosting.Configuration;
using Serilog;
using Tomlyn;

namespace NightHeaven.Plugins.Data;

/// <summary>
/// Per-plugin startup context passed to <see cref="NightHeaven.Plugins.Interfaces.INightHeavenPlugin" />.
/// </summary>
public sealed class PluginContext
{
    private readonly ILogger _logger = Log.ForContext<PluginContext>();

    public PluginContext(string pluginDirectory, DirectoriesConfig directories)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginDirectory);
        ArgumentNullException.ThrowIfNull(directories);

        PluginDirectory = Path.GetFullPath(pluginDirectory);
        PluginConfigPath = Path.Combine(PluginDirectory, "plugin.toml");
        Directories = directories;
    }

    /// <summary>Absolute directory containing the plugin package.</summary>
    public string PluginDirectory { get; }

    /// <summary>Absolute path to the optional plugin runtime TOML config.</summary>
    public string PluginConfigPath { get; }

    /// <summary>Global NightHeaven directory configuration.</summary>
    public DirectoriesConfig Directories { get; }

    /// <summary>
    /// Loads this plugin's <c>plugin.toml</c> into a typed config. Missing files are created from defaults.
    /// </summary>
    public TConfig LoadConfig<TConfig>(Func<TConfig> defaultFactory)
        where TConfig : class, new()
    {
        ArgumentNullException.ThrowIfNull(defaultFactory);
        Directory.CreateDirectory(PluginDirectory);

        if (!File.Exists(PluginConfigPath))
        {
            var defaults = defaultFactory()
                ?? throw new InvalidOperationException(
                    $"Default factory returned null for plugin config {typeof(TConfig).FullName}."
                );

            File.WriteAllText(
                PluginConfigPath,
                TomlSerializer.Serialize(defaults, ConfigTomlOptions.Instance)
            );
            _logger.Information("Created default plugin config at {Path}", PluginConfigPath);

            return defaults;
        }

        try
        {
            var text = File.ReadAllText(PluginConfigPath);
            var config = TomlSerializer.Deserialize<TConfig>(text, ConfigTomlOptions.Instance);

            return config
                ?? throw new InvalidOperationException(
                    $"Plugin config '{PluginConfigPath}' could not be parsed as {typeof(TConfig).FullName}."
                );
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Plugin config '{PluginConfigPath}' could not be parsed as {typeof(TConfig).FullName}.",
                ex
            );
        }
    }
}
```

- [ ] **Step 5: Run tests to verify pass**

Run:

```bash
dotnet test tests/NightHeaven.Tests/NightHeaven.Tests.csproj --filter "FullyQualifiedName~PluginContextTests" --nologo
```

Expected: PASS, 3 tests.

- [ ] **Step 6: Build plugin project**

Run:

```bash
dotnet build src/NightHeaven.Plugins/NightHeaven.Plugins.csproj -c Debug -nologo
```

Expected: `Build succeeded.`

- [ ] **Step 7: Commit**

```bash
git add NightHeaven.slnx src/NightHeaven.Plugins tests/NightHeaven.Tests/NightHeaven.Tests.csproj tests/NightHeaven.Tests/Plugins/PluginContextTests.cs
git commit -m "feat(plugins): add public plugin contract and context"
```

## Task 3: Add loaded plugin descriptor and dependency sorting

**Files:**

- Create: `src/NightHeaven.Plugins/Data/LoadedPlugin.cs`
- Create: `src/NightHeaven.Plugins/Internal/PluginDependencySorter.cs`
- Create: `tests/NightHeaven.Tests/Plugins/Support/FakePlugin.cs`
- Test: `tests/NightHeaven.Tests/Plugins/PluginDependencySorterTests.cs`

- [ ] **Step 1: Write fake plugin support**

Create `tests/NightHeaven.Tests/Plugins/Support/FakePlugin.cs`:

```csharp
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
```

- [ ] **Step 2: Write failing dependency tests**

Create `tests/NightHeaven.Tests/Plugins/PluginDependencySorterTests.cs`:

```csharp
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
```

- [ ] **Step 3: Run tests to verify failure**

Run:

```bash
dotnet test tests/NightHeaven.Tests/NightHeaven.Tests.csproj --filter "FullyQualifiedName~PluginDependencySorterTests" --nologo
```

Expected: FAIL because `LoadedPlugin` and `PluginDependencySorter` do not exist.

- [ ] **Step 4: Add loaded plugin descriptor**

Create `src/NightHeaven.Plugins/Data/LoadedPlugin.cs`:

```csharp
using System.Reflection;
using NightHeaven.Plugins.Interfaces;

namespace NightHeaven.Plugins.Data;

/// <summary>
/// A plugin instance loaded from a plugin package directory.
/// </summary>
public sealed class LoadedPlugin
{
    public LoadedPlugin(string pluginDirectory, INightHeavenPlugin instance, Assembly assembly)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginDirectory);
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(assembly);

        PluginDirectory = Path.GetFullPath(pluginDirectory);
        Instance = instance;
        Assembly = assembly;
        Metadata = instance.Metadata
            ?? throw new InvalidOperationException(
                $"Plugin {instance.GetType().FullName} returned null metadata."
            );
    }

    /// <summary>Absolute directory containing the plugin package.</summary>
    public string PluginDirectory { get; }

    /// <summary>The instantiated plugin.</summary>
    public INightHeavenPlugin Instance { get; }

    /// <summary>The plugin metadata.</summary>
    public PluginMetadata Metadata { get; }

    /// <summary>The assembly containing the plugin type.</summary>
    public Assembly Assembly { get; }
}
```

- [ ] **Step 5: Add dependency sorter**

Create `src/NightHeaven.Plugins/Internal/PluginDependencySorter.cs`:

```csharp
using System.Text.RegularExpressions;
using NightHeaven.Plugins.Data;

namespace NightHeaven.Plugins.Internal;

internal static partial class PluginDependencySorter
{
    public static IReadOnlyList<LoadedPlugin> ValidateAndSort(IReadOnlyList<LoadedPlugin> plugins)
    {
        ArgumentNullException.ThrowIfNull(plugins);

        var byId = new Dictionary<string, LoadedPlugin>(StringComparer.OrdinalIgnoreCase);

        foreach (var plugin in plugins)
        {
            ValidateMetadata(plugin);

            if (!byId.TryAdd(plugin.Metadata.Id, plugin))
            {
                throw new InvalidOperationException($"Duplicate plugin id '{plugin.Metadata.Id}'.");
            }
        }

        foreach (var plugin in plugins)
        {
            foreach (var dependencyId in plugin.Metadata.Dependencies)
            {
                if (string.IsNullOrWhiteSpace(dependencyId) || !PluginIdRegex().IsMatch(dependencyId))
                {
                    throw new InvalidOperationException(
                        $"Plugin '{plugin.Metadata.Id}' declares invalid dependency id '{dependencyId}'."
                    );
                }

                if (!byId.ContainsKey(dependencyId))
                {
                    throw new InvalidOperationException(
                        $"Plugin '{plugin.Metadata.Id}' has missing dependency '{dependencyId}'."
                    );
                }
            }
        }

        var ordered = new List<LoadedPlugin>(plugins.Count);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visiting = new Stack<string>();

        foreach (var plugin in plugins)
        {
            Visit(plugin.Metadata.Id, byId, ordered, visited, visiting);
        }

        return ordered;
    }

    private static void Visit(
        string id,
        IReadOnlyDictionary<string, LoadedPlugin> byId,
        List<LoadedPlugin> ordered,
        HashSet<string> visited,
        Stack<string> visiting
    )
    {
        if (visited.Contains(id))
        {
            return;
        }

        if (visiting.Contains(id, StringComparer.OrdinalIgnoreCase))
        {
            var path = string.Join(" -> ", visiting.Reverse().Append(id));
            throw new InvalidOperationException($"Plugin dependency cycle detected: {path}.");
        }

        visiting.Push(id);
        var plugin = byId[id];

        foreach (var dependencyId in plugin.Metadata.Dependencies)
        {
            Visit(dependencyId, byId, ordered, visited, visiting);
        }

        visiting.Pop();
        visited.Add(id);
        ordered.Add(plugin);
    }

    private static void ValidateMetadata(LoadedPlugin plugin)
    {
        var metadata = plugin.Metadata;

        if (string.IsNullOrWhiteSpace(metadata.Id) || !PluginIdRegex().IsMatch(metadata.Id))
        {
            throw new InvalidOperationException(
                $"Plugin at '{plugin.PluginDirectory}' has invalid plugin id '{metadata.Id}'."
            );
        }

        if (string.IsNullOrWhiteSpace(metadata.Name))
        {
            throw new InvalidOperationException($"Plugin '{metadata.Id}' has missing name.");
        }

        if (metadata.Version is null)
        {
            throw new InvalidOperationException($"Plugin '{metadata.Id}' has missing version.");
        }

        if (string.IsNullOrWhiteSpace(metadata.Author))
        {
            throw new InvalidOperationException($"Plugin '{metadata.Id}' has missing author.");
        }
    }

    [GeneratedRegex("^[a-z0-9]+(\\.[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex PluginIdRegex();
}
```

- [ ] **Step 6: Run tests to verify pass**

Run:

```bash
dotnet test tests/NightHeaven.Tests/NightHeaven.Tests.csproj --filter "FullyQualifiedName~PluginDependencySorterTests" --nologo
```

Expected: PASS, 5 tests.

- [ ] **Step 7: Commit**

```bash
git add src/NightHeaven.Plugins/Data/LoadedPlugin.cs src/NightHeaven.Plugins/Internal/PluginDependencySorter.cs tests/NightHeaven.Tests/Plugins/Support/FakePlugin.cs tests/NightHeaven.Tests/Plugins/PluginDependencySorterTests.cs
git commit -m "feat(plugins): validate plugin metadata dependencies"
```

## Task 4: Add test plugin fixture projects

**Files:**

- Modify: `NightHeaven.slnx`
- Modify: `tests/NightHeaven.Tests/NightHeaven.Tests.csproj`
- Create: `tests/NightHeaven.PluginFixtures.Basic/NightHeaven.PluginFixtures.Basic.csproj`
- Create: `tests/NightHeaven.PluginFixtures.Basic/BasicPlugin.cs`
- Create: `tests/NightHeaven.PluginFixtures.Empty/NightHeaven.PluginFixtures.Empty.csproj`
- Create: `tests/NightHeaven.PluginFixtures.Empty/EmptyMarker.cs`
- Create: `tests/NightHeaven.PluginFixtures.Multiple/NightHeaven.PluginFixtures.Multiple.csproj`
- Create: `tests/NightHeaven.PluginFixtures.Multiple/MultiplePlugins.cs`
- Create: `tests/NightHeaven.Tests/Plugins/Support/PluginFixtureCopy.cs`

- [ ] **Step 1: Add fixture projects to solution**

Modify `NightHeaven.slnx`:

```xml
<Solution>
    <Project Path="src/NightHeaven.Scripting.Lua/NightHeaven.Scripting.Lua.csproj" />
    <Project Path="src/NightHeaven.Core/NightHeaven.Core.csproj" />
    <Project Path="src/NightHeaven.Persistence/NightHeaven.Persistence.csproj" />
    <Project Path="src/NightHeaven.Hosting/NightHeaven.Hosting.csproj" />
    <Project Path="src/NightHeaven.Plugins/NightHeaven.Plugins.csproj" />
    <Project Path="src/NightHeaven.Network.UO/NightHeaven.Network.UO.csproj" />
    <Project Path="src/NightHeaven.Network/NightHeaven.Network.csproj" />
    <Project Path="src/NightHeaven.Server/NightHeaven.Server.csproj" />
    <Project Path="tests/NightHeaven.PluginFixtures.Basic/NightHeaven.PluginFixtures.Basic.csproj" />
    <Project Path="tests/NightHeaven.PluginFixtures.Empty/NightHeaven.PluginFixtures.Empty.csproj" />
    <Project Path="tests/NightHeaven.PluginFixtures.Multiple/NightHeaven.PluginFixtures.Multiple.csproj" />
    <Project Path="tests/NightHeaven.Tests/NightHeaven.Tests.csproj" />
</Solution>
```

- [ ] **Step 2: Create basic fixture project**

Create `tests/NightHeaven.PluginFixtures.Basic/NightHeaven.PluginFixtures.Basic.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <IsPackable>false</IsPackable>
    </PropertyGroup>

    <ItemGroup>
        <ProjectReference Include="..\..\src\NightHeaven.Plugins\NightHeaven.Plugins.csproj" />
        <ProjectReference Include="..\..\src\NightHeaven.Server\NightHeaven.Server.csproj" />
        <ProjectReference Include="..\..\src\NightHeaven.Scripting.Lua\NightHeaven.Scripting.Lua.csproj" />
    </ItemGroup>

</Project>
```

Create `tests/NightHeaven.PluginFixtures.Basic/BasicPlugin.cs`:

```csharp
using DryIoc;
using NightHeaven.Plugins.Data;
using NightHeaven.Plugins.Interfaces;
using NightHeaven.Scripting.Lua.Attributes.Scripts;
using NightHeaven.Server.Extensions.DryIoc;
using NightHeaven.Scripting.Lua.Extensions.Scripts;

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
```

- [ ] **Step 3: Create empty fixture project**

Create `tests/NightHeaven.PluginFixtures.Empty/NightHeaven.PluginFixtures.Empty.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <IsPackable>false</IsPackable>
    </PropertyGroup>

</Project>
```

Create `tests/NightHeaven.PluginFixtures.Empty/EmptyMarker.cs`:

```csharp
namespace NightHeaven.PluginFixtures.Empty;

public sealed class EmptyMarker;
```

- [ ] **Step 4: Create multiple fixture project**

Create `tests/NightHeaven.PluginFixtures.Multiple/NightHeaven.PluginFixtures.Multiple.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <TargetFramework>net10.0</TargetFramework>
        <ImplicitUsings>enable</ImplicitUsings>
        <Nullable>enable</Nullable>
        <IsPackable>false</IsPackable>
    </PropertyGroup>

    <ItemGroup>
        <ProjectReference Include="..\..\src\NightHeaven.Plugins\NightHeaven.Plugins.csproj" />
    </ItemGroup>

</Project>
```

Create `tests/NightHeaven.PluginFixtures.Multiple/MultiplePlugins.cs`:

```csharp
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
```

- [ ] **Step 5: Reference fixture projects from tests**

Modify `tests/NightHeaven.Tests/NightHeaven.Tests.csproj`:

```xml
<ItemGroup>
    <ProjectReference Include="..\..\src\NightHeaven.Core\NightHeaven.Core.csproj"/>
    <ProjectReference Include="..\..\src\NightHeaven.Hosting\NightHeaven.Hosting.csproj"/>
    <ProjectReference Include="..\..\src\NightHeaven.Network\NightHeaven.Network.csproj"/>
    <ProjectReference Include="..\..\src\NightHeaven.Persistence\NightHeaven.Persistence.csproj"/>
    <ProjectReference Include="..\..\src\NightHeaven.Plugins\NightHeaven.Plugins.csproj"/>
    <ProjectReference Include="..\..\src\NightHeaven.Server\NightHeaven.Server.csproj"/>
    <ProjectReference Include="..\NightHeaven.PluginFixtures.Basic\NightHeaven.PluginFixtures.Basic.csproj"/>
    <ProjectReference Include="..\NightHeaven.PluginFixtures.Empty\NightHeaven.PluginFixtures.Empty.csproj"/>
    <ProjectReference Include="..\NightHeaven.PluginFixtures.Multiple\NightHeaven.PluginFixtures.Multiple.csproj"/>
</ItemGroup>
```

- [ ] **Step 6: Create fixture copy helper**

Create `tests/NightHeaven.Tests/Plugins/Support/PluginFixtureCopy.cs`:

```csharp
namespace NightHeaven.Tests.Plugins.Support;

public static class PluginFixtureCopy
{
    public static string CopyFixture(string pluginsRoot, string assemblyName, string? directoryName = null)
    {
        var source = Path.Combine(AppContext.BaseDirectory, assemblyName + ".dll");

        if (!File.Exists(source))
        {
            throw new FileNotFoundException($"Fixture assembly '{assemblyName}' was not copied to test output.", source);
        }

        var pluginDirectory = Path.Combine(pluginsRoot, directoryName ?? assemblyName);
        Directory.CreateDirectory(pluginDirectory);
        File.Copy(source, Path.Combine(pluginDirectory, assemblyName + ".dll"), overwrite: true);

        return pluginDirectory;
    }
}
```

- [ ] **Step 7: Build fixtures**

Run:

```bash
dotnet build NightHeaven.slnx -c Debug -nologo
```

Expected: `Build succeeded.` If the fixture project fails because extension methods are not found, confirm `BasicPlugin.cs` includes both `NightHeaven.Server.Extensions.DryIoc` and `NightHeaven.Scripting.Lua.Extensions.Scripts`.

- [ ] **Step 8: Commit**

```bash
git add NightHeaven.slnx tests/NightHeaven.PluginFixtures.Basic tests/NightHeaven.PluginFixtures.Empty tests/NightHeaven.PluginFixtures.Multiple tests/NightHeaven.Tests/NightHeaven.Tests.csproj tests/NightHeaven.Tests/Plugins/Support/PluginFixtureCopy.cs
git commit -m "test(plugins): add plugin loader fixture assemblies"
```

## Task 5: Implement assembly loading and plugin scanning

**Files:**

- Create: `src/NightHeaven.Plugins/Internal/PluginAssemblyLoadContext.cs`
- Create: `src/NightHeaven.Plugins/Services/PluginLoaderService.cs`
- Test: `tests/NightHeaven.Tests/Plugins/PluginLoaderServiceTests.cs`

- [ ] **Step 1: Write failing loader tests**

Create `tests/NightHeaven.Tests/Plugins/PluginLoaderServiceTests.cs`:

```csharp
using DryIoc;
using NightHeaven.Core.Data.Directories;
using NightHeaven.Core.Types;
using NightHeaven.Plugins.Services;
using NightHeaven.Scripting.Lua.Data.Internal;
using NightHeaven.Server.Extensions.DryIoc;
using NightHeaven.Tests.Plugins.Support;

namespace NightHeaven.Tests.Plugins;

public sealed class PluginLoaderServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"nh-plugin-loader-{Guid.NewGuid():N}");

    private string PluginsRoot => Path.Combine(_root, "plugins");

    [Fact]
    public void LoadAndConfigure_MissingPluginsDirectory_CreatesDirectoryAndReturnsEmpty()
    {
        var container = new Container();
        var directories = Directories();
        var loader = new PluginLoaderService();

        var loaded = loader.LoadAndConfigure(container, directories);

        Assert.Empty(loaded);
        Assert.True(Directory.Exists(PluginsRoot));
    }

    [Fact]
    public void LoadAndConfigure_ValidPlugin_LoadsMetadataAndConfiguresContainer()
    {
        PluginFixtureCopy.CopyFixture(PluginsRoot, "NightHeaven.PluginFixtures.Basic", "basic");
        var container = new Container();
        container.AddNightHeavenLuaScripting(Directories());
        var loader = new PluginLoaderService();

        var loaded = loader.LoadAndConfigure(container, Directories());

        var plugin = Assert.Single(loaded);
        Assert.Equal("nightheaven.fixture.basic", plugin.Metadata.Id);
        Assert.True(File.Exists(Path.Combine(plugin.PluginDirectory, "plugin.toml")));
        var modules = container.Resolve<List<ScriptModuleData>>();
        Assert.Contains(modules, module => module.ModuleType.FullName == "NightHeaven.PluginFixtures.Basic.BasicPluginScriptModule");
    }

    [Fact]
    public void LoadAndConfigure_EmptyPluginDirectory_Throws()
    {
        PluginFixtureCopy.CopyFixture(PluginsRoot, "NightHeaven.PluginFixtures.Empty", "empty");
        var loader = new PluginLoaderService();

        var ex = Assert.Throws<InvalidOperationException>(
            () => loader.LoadAndConfigure(new Container(), Directories())
        );

        Assert.Contains("does not contain a plugin", ex.Message);
    }

    [Fact]
    public void LoadAndConfigure_MultiplePluginImplementations_Throws()
    {
        PluginFixtureCopy.CopyFixture(PluginsRoot, "NightHeaven.PluginFixtures.Multiple", "multiple");
        var loader = new PluginLoaderService();

        var ex = Assert.Throws<InvalidOperationException>(
            () => loader.LoadAndConfigure(new Container(), Directories())
        );

        Assert.Contains("multiple plugin implementations", ex.Message);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }

        GC.SuppressFinalize(this);
    }

    private DirectoriesConfig Directories()
        => new(_root, Enum.GetNames<DirectoryType>());
}
```

- [ ] **Step 2: Run tests to verify failure**

Run:

```bash
dotnet test tests/NightHeaven.Tests/NightHeaven.Tests.csproj --filter "FullyQualifiedName~PluginLoaderServiceTests" --nologo
```

Expected: FAIL because `PluginLoaderService` does not exist.

- [ ] **Step 3: Add plugin assembly load context**

Create `src/NightHeaven.Plugins/Internal/PluginAssemblyLoadContext.cs`:

```csharp
using System.Reflection;
using System.Runtime.Loader;

namespace NightHeaven.Plugins.Internal;

internal sealed class PluginAssemblyLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public PluginAssemblyLoadContext(string pluginDirectory)
        : base($"NightHeaven.Plugin:{Path.GetFileName(pluginDirectory)}", isCollectible: false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginDirectory);
        _resolver = new(pluginDirectory);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var shared = AssemblyLoadContext.Default.Assemblies.FirstOrDefault(
            assembly => string.Equals(
                assembly.GetName().Name,
                assemblyName.Name,
                StringComparison.OrdinalIgnoreCase
            )
        );

        if (shared is not null)
        {
            return shared;
        }

        var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);

        return assemblyPath is null ? null : LoadFromAssemblyPath(assemblyPath);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

        return libraryPath is null ? IntPtr.Zero : LoadUnmanagedDllFromPath(libraryPath);
    }
}
```

- [ ] **Step 4: Add plugin loader service**

Create `src/NightHeaven.Plugins/Services/PluginLoaderService.cs`:

```csharp
using System.Reflection;
using DryIoc;
using NightHeaven.Core.Data.Directories;
using NightHeaven.Core.Types;
using NightHeaven.Plugins.Data;
using NightHeaven.Plugins.Interfaces;
using NightHeaven.Plugins.Internal;
using Serilog;

namespace NightHeaven.Plugins.Services;

/// <summary>
/// Boot-time loader for trusted .NET plugins.
/// </summary>
public sealed class PluginLoaderService
{
    private readonly ILogger _logger = Log.ForContext<PluginLoaderService>();

    public IReadOnlyList<LoadedPlugin> LoadAndConfigure(IContainer container, DirectoriesConfig directories)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(directories);

        var pluginsDirectory = directories[DirectoryType.Plugins];
        Directory.CreateDirectory(pluginsDirectory);

        var loaded = Directory.EnumerateDirectories(pluginsDirectory)
                              .Order(StringComparer.OrdinalIgnoreCase)
                              .Select(LoadPluginDirectory)
                              .ToArray();

        var sorted = PluginDependencySorter.ValidateAndSort(loaded);

        foreach (var plugin in sorted)
        {
            var context = new PluginContext(plugin.PluginDirectory, directories);

            try
            {
                _logger.Information(
                    "Configuring plugin {PluginId} ({PluginName})",
                    plugin.Metadata.Id,
                    plugin.Metadata.Name
                );
                plugin.Instance.Configure(container, context);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Plugin '{plugin.Metadata.Id}' failed during Configure.",
                    ex
                );
            }
        }

        return sorted;
    }

    private LoadedPlugin LoadPluginDirectory(string pluginDirectory)
    {
        var dlls = Directory.EnumerateFiles(pluginDirectory, "*.dll", SearchOption.TopDirectoryOnly)
                            .Order(StringComparer.OrdinalIgnoreCase)
                            .ToArray();

        if (dlls.Length == 0)
        {
            throw new InvalidOperationException(
                $"Plugin directory '{pluginDirectory}' does not contain a plugin assembly."
            );
        }

        var loadContext = new PluginAssemblyLoadContext(pluginDirectory);
        var assemblies = new List<Assembly>(dlls.Length);

        foreach (var dll in dlls)
        {
            try
            {
                assemblies.Add(loadContext.LoadFromAssemblyPath(Path.GetFullPath(dll)));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Plugin assembly '{dll}' could not be loaded.",
                    ex
                );
            }
        }

        var pluginTypes = assemblies.SelectMany(GetLoadableTypes)
                                    .Where(type =>
                                        type is { IsAbstract: false, IsInterface: false } &&
                                        typeof(INightHeavenPlugin).IsAssignableFrom(type)
                                    )
                                    .ToArray();

        if (pluginTypes.Length == 0)
        {
            throw new InvalidOperationException(
                $"Plugin directory '{pluginDirectory}' does not contain a plugin implementation."
            );
        }

        if (pluginTypes.Length > 1)
        {
            throw new InvalidOperationException(
                $"Plugin directory '{pluginDirectory}' contains multiple plugin implementations."
            );
        }

        try
        {
            var instance = (INightHeavenPlugin?)Activator.CreateInstance(pluginTypes[0])
                ?? throw new InvalidOperationException(
                    $"Plugin type '{pluginTypes[0].FullName}' could not be instantiated."
                );

            return new(pluginDirectory, instance, pluginTypes[0].Assembly);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                $"Plugin type '{pluginTypes[0].FullName}' could not be instantiated.",
                ex
            );
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            var loaderErrors = string.Join(
                Environment.NewLine,
                ex.LoaderExceptions.Select(error => error?.Message).Where(message => message is not null)
            );

            throw new InvalidOperationException(
                $"Assembly '{assembly.FullName}' contains types that could not be loaded:{Environment.NewLine}{loaderErrors}",
                ex
            );
        }
    }
}
```

- [ ] **Step 5: Run loader tests**

Run:

```bash
dotnet test tests/NightHeaven.Tests/NightHeaven.Tests.csproj --filter "FullyQualifiedName~PluginLoaderServiceTests" --nologo
```

Expected: PASS, 4 tests.

- [ ] **Step 6: Commit**

```bash
git add src/NightHeaven.Plugins/Internal/PluginAssemblyLoadContext.cs src/NightHeaven.Plugins/Services/PluginLoaderService.cs tests/NightHeaven.Tests/Plugins/PluginLoaderServiceTests.cs
git commit -m "feat(plugins): load and configure plugin assemblies"
```

## Task 6: Integrate plugin loading into server boot

**Files:**

- Modify: `src/NightHeaven.Server/NightHeaven.Server.csproj`
- Create: `src/NightHeaven.Server/Extensions/DryIoc/PluginContainerExtensions.cs`
- Modify: `src/NightHeaven.Server/Program.cs`
- Test: `tests/NightHeaven.Tests/Plugins/PluginContainerExtensionsTests.cs`

- [ ] **Step 1: Reference plugin project from server**

Modify `src/NightHeaven.Server/NightHeaven.Server.csproj`:

```xml
<ProjectReference Include="..\NightHeaven.Plugins\NightHeaven.Plugins.csproj" />
```

The project references block should include:

```xml
<ItemGroup>
    <ProjectReference Include="..\NightHeaven.Core\NightHeaven.Core.csproj" />
    <ProjectReference Include="..\NightHeaven.Hosting\NightHeaven.Hosting.csproj" />
    <ProjectReference Include="..\NightHeaven.Network\NightHeaven.Network.csproj" />
    <ProjectReference Include="..\NightHeaven.Persistence\NightHeaven.Persistence.csproj" />
    <ProjectReference Include="..\NightHeaven.Plugins\NightHeaven.Plugins.csproj" />
    <ProjectReference Include="..\NightHeaven.Network.UO\NightHeaven.Network.UO.csproj" />
    <ProjectReference Include="..\NightHeaven.Scripting.Lua\NightHeaven.Scripting.Lua.csproj" />
</ItemGroup>
```

- [ ] **Step 2: Write failing container extension test**

Create `tests/NightHeaven.Tests/Plugins/PluginContainerExtensionsTests.cs`:

```csharp
using DryIoc;
using NightHeaven.Core.Data.Directories;
using NightHeaven.Core.Types;
using NightHeaven.Hosting.Configuration;
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
```

- [ ] **Step 3: Run test to verify failure**

Run:

```bash
dotnet test tests/NightHeaven.Tests/NightHeaven.Tests.csproj --filter "FullyQualifiedName~PluginContainerExtensionsTests" --nologo
```

Expected: FAIL because `AddNightHeavenPlugins` does not exist.

- [ ] **Step 4: Add DryIoc plugin extension**

Create `src/NightHeaven.Server/Extensions/DryIoc/PluginContainerExtensions.cs`:

```csharp
using DryIoc;
using NightHeaven.Core.Data.Directories;
using NightHeaven.Plugins.Services;

namespace NightHeaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helper for boot-time .NET plugins.
/// </summary>
public static class PluginContainerExtensions
{
    /// <summary>
    /// Loads trusted .NET plugins from the configured plugins directory and lets them register into the container.
    /// Must run before <see cref="ConfigContainerExtensions.AddNightHeavenConfig" />.
    /// </summary>
    public static IContainer AddNightHeavenPlugins(this IContainer container, DirectoriesConfig directoriesConfig)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(directoriesConfig);

        var loader = new PluginLoaderService();
        loader.LoadAndConfigure(container, directoriesConfig);

        return container;
    }
}
```

- [ ] **Step 5: Wire `Program.cs` boot order**

Modify `src/NightHeaven.Server/Program.cs` so the plugin loader runs after built-in script modules and before global config:

```csharp
container.RegisterScriptModule<LogModule>();

// Plugins can declare config sections, services, Lua modules, persistence entities, and handlers.
// This must run before AddNightHeavenConfig so plugin config sections are bound at boot.
container.AddNightHeavenPlugins(directoriesConfig);

// Load config.toml once and register every section as a DI instance. Must run after
// all RegisterConfigSection calls (each module helper declares its section).
container.AddNightHeavenConfig(
    Path.Combine(directoriesConfig[DirectoryType.Config], "nightheaven.toml")
);
```

- [ ] **Step 6: Run integration test**

Run:

```bash
dotnet test tests/NightHeaven.Tests/NightHeaven.Tests.csproj --filter "FullyQualifiedName~PluginContainerExtensionsTests" --nologo
```

Expected: PASS, 1 test.

- [ ] **Step 7: Build server**

Run:

```bash
dotnet build src/NightHeaven.Server/NightHeaven.Server.csproj -c Debug -nologo
```

Expected: `Build succeeded.`

- [ ] **Step 8: Commit**

```bash
git add src/NightHeaven.Server/NightHeaven.Server.csproj src/NightHeaven.Server/Extensions/DryIoc/PluginContainerExtensions.cs src/NightHeaven.Server/Program.cs tests/NightHeaven.Tests/Plugins/PluginContainerExtensionsTests.cs
git commit -m "feat(plugins): wire plugin loading into server boot"
```

## Task 7: Final verification and runtime smoke

**Files:**

- No new files.
- Verifies all code added in previous tasks.

- [ ] **Step 1: Run full test suite**

Run:

```bash
dotnet test tests/NightHeaven.Tests/NightHeaven.Tests.csproj -c Release --nologo
```

Expected: PASS. The total test count will be higher than the current 458 because plugin tests were added.

- [ ] **Step 2: Build full solution**

Run:

```bash
dotnet build NightHeaven.slnx -c Release -nologo
```

Expected: `Build succeeded.`

- [ ] **Step 3: Smoke server with empty plugin directory**

Run:

```bash
rm -rf /tmp/nightheaven-plugin-smoke
timeout 8s env NIGHTHEAVEN_ROOT=/tmp/nightheaven-plugin-smoke ASPNETCORE_URLS=http://127.0.0.1:0 dotnet run --project src/NightHeaven.Server/NightHeaven.Server.csproj -c Release --no-launch-profile
```

Expected: exit code `124` from `timeout`, logs show normal startup/shutdown, and `/tmp/nightheaven-plugin-smoke/plugins` exists.

- [ ] **Step 4: Build Docker image**

Run:

```bash
docker build -f src/NightHeaven.Server/Dockerfile -t nightheaven-server-plugin-check .
```

Expected: Docker build succeeds. Existing repository warnings are acceptable if there are no errors.

- [ ] **Step 5: Commit if smoke-only doc/test adjustments were needed**

If Task 7 required no file changes, do not commit. If a small fix was needed, stage only the changed files and commit:

```bash
git add <changed-files>
git commit -m "fix(plugins): complete plugin startup verification"
```

## Self-Review Checklist

- Spec coverage:
  - Public plugin contract: Tasks 1-2.
  - Metadata from code: Tasks 1 and 3.
  - `plugin.toml` runtime config only: Task 2 and Basic fixture in Task 4.
  - Fail-fast loader: Tasks 3 and 5.
  - Dependency validation/topological order: Task 3.
  - Assembly loading from `plugins/*`: Tasks 4-5.
  - Boot integration before `AddNightHeavenConfig`: Task 6.
  - Empty/missing directory behavior: Task 5 and Task 7.
- Placeholder scan: no `TBD`, no deferred implementation steps.
- Type consistency:
  - `INightHeavenPlugin.Configure(IContainer container, PluginContext context)` is used consistently.
  - `PluginMetadata.Dependencies` is used consistently.
  - `PluginLoaderService.LoadAndConfigure(IContainer container, DirectoriesConfig directories)` is used consistently.
