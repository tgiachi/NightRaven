using NightRaven.Abstractions.Interfaces.Events;

namespace NightRaven.Tests.Hosting.EventBus.Support;

internal sealed record TestAsyncEvent(string Payload) : IAsyncEvent;

internal sealed record TestTickEvent(int Value) : ITickEvent;
