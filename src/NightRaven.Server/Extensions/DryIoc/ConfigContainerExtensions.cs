using DryIoc;
using NightRaven.Abstractions.Configuration;
using NightRaven.Abstractions.Data.Internal;

namespace NightRaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native bootstrap helpers for the NightRaven TOML config system.
/// </summary>
public static class ConfigContainerExtensions
{
    /// <summary>
    /// Loads the TOML config file once and registers every bound section as a DI instance. Must be
    /// the last config call, after every config-section declaration.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    /// <param name="configFilePath">Full path to the TOML config file.</param>
    public static IContainer AddNightRavenConfig(this IContainer container, string configFilePath)
    {
        if (!container.IsRegistered<List<ConfigSectionRegistration>>())
        {
            return container;
        }

        var sections = container.Resolve<List<ConfigSectionRegistration>>();

        if (sections.Count == 0)
        {
            return container;
        }

        foreach (var result in ConfigService.Load(configFilePath, sections))
        {
            container.RegisterInstance(result.Type, result.Instance);
        }

        return container;
    }
}
