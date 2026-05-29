using NightHeaven.Hosting.Interfaces.Services;

namespace NightHeaven.Hosting.Internal;

/// <summary>
/// Pairs a registered <see cref="INightHeavenService" /> with its start priority.
/// Lower priorities start first; stop happens in reverse order.
/// </summary>
internal sealed record NightHeavenServiceDescriptor(INightHeavenService Service, int Priority);
