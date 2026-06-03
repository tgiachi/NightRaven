# .NET Plugin System Design

## Goal

Add a boot-time .NET plugin system for NightHeaven. Plugins live under the server
`plugins/` directory, expose metadata from code, may read their own optional
`plugin.toml`, and can register NightHeaven services, config sections, Lua
modules, persistence entities, packet handlers, metrics, and event handlers
before the server container is built.

## Non-Goals

- No hot reload.
- No plugin unloading.
- No sandboxing. Plugins are trusted .NET code.
- No dependency version constraints in v1.
- No web endpoint registration API in v1.
- No plugin marketplace or package manager in v1.

## Public Plugin Contract

Create a public plugin API that external plugin projects can reference:

```csharp
public interface INightHeavenPlugin
{
    PluginMetadata Metadata { get; }

    void Configure(IContainer container, PluginContext context);
}
```

`Metadata` is the source of truth for plugin identity and descriptive
information. `plugin.toml` must not duplicate this identity data.

```csharp
public sealed class PluginMetadata
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required Version Version { get; init; }
    public required string Author { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<string> Dependencies { get; init; } = [];
}
```

Metadata validation is fail-fast:

- `Id`, `Name`, `Author`, and `Version` are required.
- `Id` must be unique across loaded plugins.
- `Id` should use lowercase dotted identifiers, for example
  `nightheaven.weather`.
- `Dependencies` contains plugin IDs only, with no version constraints in v1.

## Plugin Package Layout

Each plugin is a directory under `plugins/`:

```text
plugins/
  nightheaven.weather/
    NightHeaven.WeatherPlugin.dll
    plugin.toml
    dependency-a.dll
```

The folder name is a package location, not the plugin identity. The plugin ID is
read from `INightHeavenPlugin.Metadata.Id`.

The loader scans top-level `*.dll` files in each plugin directory, loads them
with an assembly resolver rooted at that directory, and discovers
`INightHeavenPlugin` implementations. Each plugin directory must contain exactly
one concrete plugin implementation.

## Plugin Configuration

`plugin.toml` is optional and contains plugin runtime settings only:

```toml
weather_interval_seconds = 2
enabled_regions = ["Britannia", "Trammel"]
```

`PluginContext` exposes the plugin root directory, `plugin.toml` path, the global
`DirectoriesConfig`, and a helper for reading typed plugin config:

```csharp
public sealed class PluginContext
{
    public string PluginDirectory { get; }
    public string PluginConfigPath { get; }
    public DirectoriesConfig Directories { get; }

    public TConfig LoadConfig<TConfig>(Func<TConfig> defaultFactory)
        where TConfig : class, new();
}
```

`LoadConfig<TConfig>` uses the same TOML serializer options as server config. If
`plugin.toml` does not exist, it returns the default config and writes the default
file. If the file exists and cannot be parsed into `TConfig`, server startup
fails.

## Loader Service

`PluginLoaderService` is a boot-time service class, not an `IHostedService`.
`Program.cs` calls it during DryIoc container configuration.

Responsibilities:

1. Ensure the plugins directory exists.
2. Scan `plugins/*` directories.
3. Load plugin assemblies from each directory.
4. Discover exactly one concrete `INightHeavenPlugin` per plugin directory.
5. Instantiate each plugin.
6. Read and validate each plugin's `Metadata`.
7. Reject duplicate IDs.
8. Reject missing dependencies.
9. Reject dependency cycles.
10. Sort plugins topologically by dependency ID.
11. Call `Configure(container, context)` in dependency order.

Any load, validation, dependency, config, or configure error fails server startup.

## Boot Order

The plugin loader runs after built-in services and built-in Lua modules are
registered, but before the single server TOML config is loaded.

```text
Program.cs
  AddNightHeavenEventBus()
  AddNightHeavenTimerWheel()
  AddNightHeavenMetrics()
  AddNightHeavenPersistence(...)
  AddNightHeavenNetwork()
  AddNightHeavenLuaScripting(...)
  RegisterScriptModule<LogModule>()
  AddNightHeavenPlugins(directoriesConfig)
  AddNightHeavenConfig(config/nightheaven.toml)
  builder.Build()
```

This order lets plugins declare their own config sections before
`AddNightHeavenConfig` binds all registered config.

## Plugin Capabilities

Inside `Configure`, a plugin can use the same registration APIs as built-in
modules:

- `RegisterConfigSection<TConfig>(...)`
- `AddNightHeavenService<TInterface, TImplementation>(priority)`
- `AddTickEventHandler<THandler, TEvent>()`
- `AddAsyncEventHandler<THandler, TEvent>()`
- `RegisterScriptModule<TScriptModule>()`
- `RegisterLuaUserData<TUserData>()`
- `RegisterPersistenceEntity<TEntity, TKey>(...)`
- `AddMetricProvider<TProvider>()`

Plugins are responsible for choosing service priorities that do not conflict
with their intended runtime order. Equal priorities preserve registration order.

## Assembly Loading

Use a per-plugin non-collectible `AssemblyLoadContext` with
`AssemblyDependencyResolver` rooted at the plugin directory. This keeps plugin
dependency probing local to the plugin package while matching the v1 decision to
avoid unload/hot reload.

The loader should still avoid loading the main NightHeaven assemblies from the
plugin directory when those assemblies are already available in the host. Plugin
projects should reference the host API assemblies but should not copy duplicate
host assemblies into the plugin package.

## Error Handling

Startup fails with clear messages for:

- plugin directory contains no plugin implementation
- plugin directory contains multiple plugin implementations
- metadata field is missing or invalid
- duplicate plugin ID
- dependency plugin ID is missing
- dependency cycle
- assembly load failure
- plugin constructor failure
- plugin config parse failure
- `Configure` exception

Errors should include the plugin directory path and plugin ID when available.

## Testing Strategy

Unit and integration tests cover:

- empty plugins directory succeeds
- missing plugin directory is created
- plugin directory without plugin implementation fails
- duplicate plugin IDs fail
- missing dependencies fail
- dependency cycles fail
- dependency order controls configure order
- plugin `Configure` can register config sections before global config load
- plugin `Configure` can register a script module
- plugin config defaults are written when `plugin.toml` is missing
- malformed `plugin.toml` fails startup

Test plugin assemblies should be generated or copied into temporary plugin
directories so the loader is tested through real assembly loading, not only
in-memory types.

## Open Decisions Locked for V1

- Dependencies are plugin IDs only.
- Plugin metadata comes from `INightHeavenPlugin.Metadata`.
- `plugin.toml` is plugin-specific runtime config only.
- Loader is fail-fast.
- Loading happens only at boot.
- Plugins are trusted code.
