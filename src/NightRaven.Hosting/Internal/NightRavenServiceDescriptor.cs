using NightRaven.Hosting.Interfaces.Services;

namespace NightRaven.Hosting.Internal;

/// <summary>
/// Pairs a registered <see cref="INightRavenService" /> with its start priority.
/// Lower priorities start first; stop happens in reverse order.
/// </summary>
internal sealed record NightRavenServiceDescriptor(INightRavenService Service, int Priority);
