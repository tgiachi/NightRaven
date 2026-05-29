using Tomlyn.Serialization;

namespace NightHeaven.Tests.Core.Toml;

[TomlSerializable(typeof(TomlPerson))]
public partial class TomlTestContext : TomlSerializerContext;
