using DryIoc;
using NightHeaven.Core.Extensions.Container;
using NightHeaven.Hosting.Configuration;
using NightHeaven.Hosting.Data.Internal;

namespace NightHeaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for the NightHeaven TOML config system.
/// </summary>
public static class ConfigContainerExtensions
{
    /// <summary>
    /// Declares a config section. Accumulates a descriptor consumed by
    /// <see cref="AddNightHeavenConfig" /> at boot. Call before <see cref="AddNightHeavenConfig" />.
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
            () => defaultFactory()
        );
        container.AddToRegisterTypedList(registration);

        return container;
    }

    /// <summary>
    /// Loads the TOML config file once and registers every bound section as a DI instance. Must be
    /// the last config call (after every <see cref="RegisterConfigSection{TConfig}" />).
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    /// <param name="configFilePath">Full path to the TOML config file.</param>
    public static IContainer AddNightHeavenConfig(this IContainer container, string configFilePath)
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
