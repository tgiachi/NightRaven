using NightRaven.Abstractions.Interfaces.Events;

namespace NightRaven.Tests.Hosting.EventBus.Support;

internal sealed record TestAsyncEvent : IAsyncEvent
{
    public string Payload { get; }

    public TestAsyncEvent(string payload)
    {
        Payload = payload;
    }
}
