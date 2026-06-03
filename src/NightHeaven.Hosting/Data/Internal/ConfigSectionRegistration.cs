namespace NightHeaven.Hosting.Data.Internal;

/// <summary>
/// Boot-time declaration of a config section: how to name it, default it, and bind it from TOML.
/// Accumulated in the container and consumed by the config loader at startup.
/// </summary>
public sealed class ConfigSectionRegistration
{
    private readonly Func<object> _defaultFactory;
    private readonly Func<string, object> _bind;

    public ConfigSectionRegistration(
        string name,
        Type type,
        Func<object> defaultFactory,
        Func<string, object> bind
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(defaultFactory);
        ArgumentNullException.ThrowIfNull(bind);

        Name = name;
        Type = type;
        _defaultFactory = defaultFactory;
        _bind = bind;
    }

    /// <summary>TOML table name for this section (e.g. <c>persistence</c>).</summary>
    public string Name { get; }

    /// <summary>CLR type of the config.</summary>
    public Type Type { get; }

    /// <summary>Creates a fresh default instance of the config.</summary>
    public object CreateDefault()
        => _defaultFactory();

    /// <summary>Binds a TOML section body to a typed config instance.</summary>
    public object Bind(string sectionToml)
        => _bind(sectionToml);
}
