using Tomlyn.Serialization;

namespace NightRaven.Tests.Core.Toml;

[TomlSerializable(typeof(TomlPerson))]
public partial class TomlTestContext : TomlSerializerContext;
