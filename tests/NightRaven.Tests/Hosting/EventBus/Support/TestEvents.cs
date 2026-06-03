using NightHeaven.Hosting.Interfaces.Events;

namespace NightHeaven.Tests.Hosting.EventBus.Support;

internal sealed record TestAsyncEvent(string Payload) : IAsyncEvent;

internal sealed record TestTickEvent(int Value) : ITickEvent;
