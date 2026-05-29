using NightHeaven.Hosting.Interfaces.EventHandlers;
using NightHeaven.Hosting.Interfaces.Events;
using NightHeaven.Hosting.Interfaces.Services;

namespace NightHeaven.Tests.Hosting.EventBus.Support;

internal sealed record TestAsyncEvent(string Payload) : IAsyncEvent;

internal sealed record TestTickEvent(int Value) : ITickEvent;
