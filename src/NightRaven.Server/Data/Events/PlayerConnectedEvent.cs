using NightHeaven.Hosting.Interfaces.Events;

namespace NightHeaven.Server.Data.Events;

/// <summary>
/// Tick event published when a client connects and a session is created.
/// </summary>
public sealed record PlayerConnectedEvent(long SessionId, string? RemoteEndPoint, DateTimeOffset At) : ITickEvent;
