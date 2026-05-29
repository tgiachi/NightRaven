using NightHeaven.Hosting.Interfaces.EventHandlers;
using NightHeaven.Hosting.Interfaces.Events;
using NightHeaven.Hosting.Interfaces.Services;

namespace NightHeaven.Tests.Hosting.EventBus.Support;

internal sealed class ThrowingAsyncHandler : IAsyncEventHandler<TestAsyncEvent>
{
    public Task HandleAsync(TestAsyncEvent evt, CancellationToken cancellationToken)
        => throw new InvalidOperationException("async boom");
}

internal sealed class ThrowingTickHandler : ITickEventHandler<TestTickEvent>
{
    public void Handle(TestTickEvent evt)
        => throw new InvalidOperationException("tick boom");
}
