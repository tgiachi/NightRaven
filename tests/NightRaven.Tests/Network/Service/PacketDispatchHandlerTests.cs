using DryIoc;
using NightRaven.Hosting.Interfaces.EventHandlers;
using NightRaven.Hosting.Data.Network;
using NightRaven.Hosting.Interfaces.Network;
using NightRaven.Hosting.Interfaces.Services;
using NightRaven.Network.Client;
using NightRaven.Network.Spans;
using NightRaven.Network.UO.Base;
using NightRaven.Network.UO.Registry;
using NightRaven.Server.Data.Events;
using NightRaven.Server.Extensions.DryIoc;
using NightRaven.Server.Interfaces.Network;
using NightRaven.Server.Services.Network;
using NightRaven.Server.Services.Network.Internal;

namespace NightRaven.Tests.Network.Service;

public class PacketDispatchHandlerTests
{
    [Fact]
    public void Handle_MatchingPacket_InvokesTypedHandler()
    {
        var handler = new CapturingHandler();
        var dispatcher = NewDispatcher([handler]);

        dispatcher.Handle(new PacketReceivedEvent(10, 0xA1, new TestPacket(0xA1), DateTimeOffset.UtcNow));

        Assert.Equal(new long[] { 10 }, handler.SessionIds);
    }

    [Fact]
    public void Handle_NonMatchingPacket_DoesNotInvokeUnrelatedHandler()
    {
        var handler = new CapturingHandler();
        var dispatcher = NewDispatcher([handler]);

        dispatcher.Handle(new PacketReceivedEvent(10, 0xA2, new OtherPacket(), DateTimeOffset.UtcNow));

        Assert.Empty(handler.SessionIds);
    }

    [Fact]
    public void Handle_MultipleMatchingHandlers_InvokesAll()
    {
        var first = new CapturingHandler();
        var second = new CapturingHandler();
        var dispatcher = NewDispatcher([first, second]);

        dispatcher.Handle(new PacketReceivedEvent(10, 0xA1, new TestPacket(0xA1), DateTimeOffset.UtcNow));

        Assert.Equal(new long[] { 10 }, first.SessionIds);
        Assert.Equal(new long[] { 10 }, second.SessionIds);
    }

    [Fact]
    public void Handle_HandlerThrows_ContinuesWithRemainingHandlers()
    {
        var capturing = new CapturingHandler();
        var dispatcher = NewDispatcher([new ThrowingHandler(), capturing]);

        dispatcher.Handle(new PacketReceivedEvent(10, 0xA1, new TestPacket(0xA1), DateTimeOffset.UtcNow));

        Assert.Equal(new long[] { 10 }, capturing.SessionIds);
    }

    [Fact]
    public void AddPacketHandler_RegistersHandlerMapping()
    {
        var container = new Container();

        container.AddPacketHandler<CapturingHandler, TestPacket>();

        var handlers = container.ResolveMany<IPacketHandler<TestPacket>>().ToArray();

        Assert.Single(handlers);
        Assert.IsType<CapturingHandler>(handlers[0]);
    }

    [Fact]
    public void AddPacketHandlersFromAssembly_RegistersHandlers()
    {
        var container = new Container();

        container.AddPacketHandlersFromAssembly(typeof(ScannedHandler).Assembly);

        var handlers = container.ResolveMany<IPacketHandler<ScanPacket>>().ToArray();

        Assert.Contains(handlers, static handler => handler.GetType() == typeof(ScannedHandler));
    }

    [Fact]
    public void AddNightRavenPacketHandlers_RegistersDispatcherAsTickHandler()
    {
        var container = new Container();
        container.Register<IOutgoingPacketQueue, OutgoingPacketQueue>(Reuse.Singleton);
        container.Register<ISessionService, SessionService>(Reuse.Singleton);

        container.AddNightRavenPacketHandlers();

        var handlers = container.ResolveMany<ITickEventHandler<PacketReceivedEvent>>().ToArray();

        Assert.Contains(handlers, static handler => handler.GetType() == typeof(PacketDispatchHandler));
    }

    [Fact]
    public void EventBus_DrainsPacketReceivedEvent_InvokesTypedHandler()
    {
        var container = new Container();
        container.RegisterInstance(new PacketRegistry());
        container.RegisterInstance(new IntegrationCapture());
        container.AddNightRavenEventBus();
        container.AddNightRavenNetwork();
        container.AddNightRavenPacketHandlers();
        container.AddPacketHandler<IntegrationHandler, TestPacket>();

        var bus = container.Resolve<IEventBusService>();

        bus.Publish(new PacketReceivedEvent(10, 0xA1, new TestPacket(0xA1), DateTimeOffset.UtcNow));
        var processed = bus.DrainTickEvents(10);

        Assert.Equal(1, processed);
        Assert.Equal(new long[] { 10 }, container.Resolve<IntegrationCapture>().SessionIds);
    }

    private static PacketDispatchHandler NewDispatcher(IReadOnlyList<IPacketHandler<TestPacket>> handlers)
        => new(
            new FakeServiceProvider(handlers),
            new OutgoingPacketQueue(),
            new SessionService()
        );

    private sealed class CapturingHandler : IPacketHandler<TestPacket>
    {
        public List<long> SessionIds { get; } = [];

        public Task HandleAsync(PacketContext<TestPacket> context, CancellationToken cancellationToken = default)
        {
            SessionIds.Add(context.SessionId);

            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingHandler : IPacketHandler<TestPacket>
    {
        public Task HandleAsync(PacketContext<TestPacket> context, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("handler failed");
    }

    private sealed class ScannedHandler : IPacketHandler<ScanPacket>
    {
        public Task HandleAsync(PacketContext<ScanPacket> context, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeServiceProvider : IServiceProvider
    {
        private readonly IReadOnlyList<IPacketHandler<TestPacket>> _handlers;

        public FakeServiceProvider(IReadOnlyList<IPacketHandler<TestPacket>> handlers)
        {
            _handlers = handlers;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IEnumerable<IPacketHandler<TestPacket>>))
            {
                return _handlers;
            }

            return null;
        }
    }

    private sealed class IntegrationCapture
    {
        public List<long> SessionIds { get; } = [];
    }

    private sealed class IntegrationHandler : IPacketHandler<TestPacket>
    {
        private readonly IntegrationCapture _capture;

        public IntegrationHandler(IntegrationCapture capture)
        {
            _capture = capture;
        }

        public Task HandleAsync(PacketContext<TestPacket> context, CancellationToken cancellationToken = default)
        {
            _capture.SessionIds.Add(context.SessionId);

            return Task.CompletedTask;
        }
    }

    private sealed class TestPacket : BaseGameNetworkPacket
    {
        public TestPacket(byte opCode)
            : base(opCode, 1) { }

        public override void Write(ref SpanWriter writer)
            => writer.Write(OpCode);

        protected override bool ParsePayload(ref SpanReader reader)
            => true;
    }

    private sealed class OtherPacket : BaseGameNetworkPacket
    {
        public OtherPacket()
            : base(0xA2, 1) { }

        public override void Write(ref SpanWriter writer)
            => writer.Write(OpCode);

        protected override bool ParsePayload(ref SpanReader reader)
            => true;
    }

    private sealed class ScanPacket : BaseGameNetworkPacket
    {
        public ScanPacket()
            : base(0xA3, 1) { }

        public override void Write(ref SpanWriter writer)
            => writer.Write(OpCode);

        protected override bool ParsePayload(ref SpanReader reader)
            => true;
    }
}
