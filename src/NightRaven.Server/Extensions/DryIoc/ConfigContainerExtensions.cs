using DryIoc;
using NightRaven.Core.Extensions.Container;
using NightRaven.Hosting.Configuration;
using NightRaven.Hosting.Data.Internal;

namespace NightRaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for the NightRaven TOML config system.
/// </summary>
public static class ConfigContainerExtensions
{
    /// <summary>
    /// Loads the TOML config file once and registers every bound section as a DI instance. Must be
    /// the last config call (after every <see cref="RegisterConfigSection{TConfig}" />).
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

    /// <summary>
    /// Declares a config section. Accumulates a descriptor consumed by
    /// <see cref="AddNightRavenConfig" /> at boot. Call before <see cref="AddNightRavenConfig" />.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    /// <param name="name">TOML section name (e.g. <c>persistence</c>).</param>
    /// <param name="defaultFactory">Creates a fresh default instance.</param>
    public static IContainer RegisterConfigSection<TConfig>(
        this IContainer container,
        string name,
        Func<TConfig> defaultFactory
    )
        where TConfig : class, new()
    {
        ArgumentNullException.ThrowIfNull(defaultFactory);

        var registration = new ConfigSectionRegistration(
            name,
            typeof(TConfig),
            defaultFactory
        );
        container.AddToRegisterTypedList(registration);

        return container;
    }
}
