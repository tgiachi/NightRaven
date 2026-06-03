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
