using System.Reflection;
using NightRaven.Plugins.Interfaces;

namespace NightRaven.Plugins.Data;

/// <summary>
/// A plugin instance loaded from a plugin package directory.
/// </summary>
public sealed class LoadedPlugin
{
    public LoadedPlugin(string pluginDirectory, INightRavenPlugin instance, Assembly assembly)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginDirectory);
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(assembly);

        PluginDirectory = Path.GetFullPath(pluginDirectory);
        Instance = instance;
        Assembly = assembly;
        Metadata = instance.Metadata ??
                   throw new InvalidOperationException($"Plugin {instance.GetType().FullName} returned null metadata.");
    }

    /// <summary>Absolute directory containing the plugin package.</summary>
    public string PluginDirectory { get; }

    /// <summary>The instantiated plugin.</summary>
    public INightRavenPlugin Instance { get; }

    /// <summary>The plugin metadata.</summary>
    public PluginMetadata Metadata { get; }

    /// <summary>The assembly containing the plugin type.</summary>
    public Assembly Assembly { get; }
}
