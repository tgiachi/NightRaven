namespace NightRaven.Abstractions.Interfaces.Events;

/// <summary>
/// Marker base interface for every message routed through NightRaven's event bus.
/// </summary>
/// <remarks>
/// Concrete events MUST implement either <see cref="IAsyncEvent" /> (thread-pool routing)
/// or <see cref="ITickEvent" /> (deterministic game-loop routing). Implementing this
/// interface directly is not supported.
/// </remarks>
public interface INightRavenEvent { }
