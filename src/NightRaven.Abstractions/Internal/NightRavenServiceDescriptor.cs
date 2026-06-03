using NightRaven.Abstractions.Interfaces.Services;

namespace NightRaven.Abstractions.Internal;

/// <summary>
/// Pairs a registered <see cref="INightRavenService" /> with its start priority.
/// Lower priorities start first; stop happens in reverse order.
/// </summary>
internal sealed record NightRavenServiceDescriptor(INightRavenService Service, int Priority);
