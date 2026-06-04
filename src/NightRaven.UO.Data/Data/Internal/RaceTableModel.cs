using NightRaven.UO.Data.Data.Races;

namespace NightRaven.UO.Data.Data.Internal;

/// <summary>TOML root binding for <c>races.toml</c> (array of tables <c>[[race]]</c>).</summary>
public sealed class RaceTableModel
{
    public List<RaceDefinition> Race { get; set; } = [];
}
