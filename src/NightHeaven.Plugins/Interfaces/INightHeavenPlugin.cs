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
