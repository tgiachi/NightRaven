using NightHeaven.Hosting.Interfaces.Events;

namespace NightHeaven.Server.Data.Events;

/// <summary>
/// Tick event published when a client disconnects and its session is removed.
/// </summary>
public sealed record PlayerDisconnectedEvent(long SessionId, string? RemoteEndPoint, DateTimeOffset At) : ITickEvent;
