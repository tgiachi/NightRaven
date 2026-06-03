namespace NightHeaven.Hosting.Interfaces.Events;

/// <summary>
/// Marker base interface for every message routed through NightHeaven's event bus.
/// </summary>
/// <remarks>
/// Concrete events MUST implement either <see cref="IAsyncEvent" /> (thread-pool routing)
/// or <see cref="ITickEvent" /> (deterministic game-loop routing). Implementing this
/// interface directly is not supported.
/// </remarks>
public interface INightHeavenEvent { }
