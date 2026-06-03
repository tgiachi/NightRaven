using NightRaven.Hosting.Interfaces.Events;

namespace NightRaven.Server.Data.Events;

/// <summary>
/// Tick event published when a client connects and a session is created.
/// </summary>
public sealed record PlayerConnectedEvent(long SessionId, string? RemoteEndPoint, DateTimeOffset At) : ITickEvent;
