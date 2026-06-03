using DryIoc;
using NightRaven.Core.Data.Directories;
using NightRaven.Server.Extensions.Configuration;
using NightRaven.Plugins.Services;

namespace NightRaven.Server.Extensions.Plugins;

/// <summary>
/// DryIoc-native registration helper for boot-time .NET plugins.
/// </summary>
public static class PluginContainerExtensions
{
    /// <summary>
    /// Loads trusted .NET plugins from the configured plugins directory and lets them register into the container.
    /// Must run before <see cref="ConfigContainerExtensions.AddNightRavenConfig" />.
    /// </summary>
    public static IContainer AddNightRavenPlugins(this IContainer container, DirectoriesConfig directoriesConfig)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(directoriesConfig);

        var loader = new PluginLoaderService();
        loader.LoadAndConfigure(container, directoriesConfig);

        return container;
    }
}
