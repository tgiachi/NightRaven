using NightRaven.Abstractions.Interfaces.Events;

namespace NightRaven.Tests.Hosting.EventBus.Support;

internal sealed record TestTickEvent : ITickEvent
{
    public int Value { get; }

    public TestTickEvent(int value)
    {
        Value = value;
    }
}
