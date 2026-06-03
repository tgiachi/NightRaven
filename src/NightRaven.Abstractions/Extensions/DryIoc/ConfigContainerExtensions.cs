using DryIoc;
using NightRaven.Abstractions.Data.Internal;
using NightRaven.Core.Extensions.Container;

namespace NightRaven.Abstractions.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for NightRaven config declarations.
/// </summary>
public static class ConfigContainerExtensions
{
    /// <summary>
    /// Declares a config section consumed by the server config loader at boot.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    /// <param name="name">TOML section name.</param>
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
