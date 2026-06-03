using System.Collections.Concurrent;
using System.Reflection;
using NightRaven.Abstractions.Data.Network;
using NightRaven.Abstractions.Interfaces.EventHandlers;
using NightRaven.Abstractions.Interfaces.Network;
using NightRaven.Network.UO.Interfaces;
using NightRaven.Server.Data.Events;
using NightRaven.Server.Interfaces.Network;
using Serilog;
using ILogger = Serilog.ILogger;

namespace NightRaven.Server.Services.Network;

/// <summary>
/// Dispatches parsed packet events to typed packet handlers registered in DI.
/// </summary>
public sealed class PacketDispatchHandler : ITickEventHandler<PacketReceivedEvent>
{
    private static readonly MethodInfo DispatchMethod = typeof(PacketDispatchHandler).GetMethod(
        nameof(Dispatch),
        BindingFlags.Instance | BindingFlags.NonPublic
    )!;

    private readonly ILogger _logger = Log.ForContext<PacketDispatchHandler>();
    private readonly IServiceProvider _serviceProvider;
    private readonly IOutgoingPacketQueue _outgoingPackets;
    private readonly ISessionService _sessions;
    private readonly ConcurrentDictionary<Type, Action<PacketDispatchHandler, PacketReceivedEvent>> _dispatchers = new();

    public PacketDispatchHandler(
        IServiceProvider serviceProvider,
        IOutgoingPacketQueue outgoingPackets,
        ISessionService sessions
    )
    {
        _serviceProvider = serviceProvider;
        _outgoingPackets = outgoingPackets;
        _sessions = sessions;
    }

    public void Handle(PacketReceivedEvent evt)
    {
        ArgumentNullException.ThrowIfNull(evt);

        var dispatcher = _dispatchers.GetOrAdd(evt.Packet.GetType(), CreateDispatcher);
        dispatcher(this, evt);
    }

    private static Action<PacketDispatchHandler, PacketReceivedEvent> CreateDispatcher(Type packetType)
    {
        var closedMethod = DispatchMethod.MakeGenericMethod(packetType);

        return (handler, evt) => closedMethod.Invoke(handler, [evt]);
    }

    private void Dispatch<TPacket>(PacketReceivedEvent evt)
        where TPacket : IGameNetworkPacket
    {
        var handlers = _serviceProvider.GetService<IEnumerable<IPacketHandler<TPacket>>>() ?? [];

        var context = new PacketContext<TPacket>(
            evt.SessionId,
            (TPacket)evt.Packet,
            evt.At,
            EnqueuePacketAsync,
            GetSessionIds
        );

        foreach (var handler in handlers)
        {
            try
            {
                handler.HandleAsync(context).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.Error(
                    ex,
                    "Packet handler {Handler} failed for {Packet}",
                    handler.GetType().Name,
                    typeof(TPacket).Name
                );
            }
        }
    }

    private Task EnqueuePacketAsync(
        long sessionId,
        IGameNetworkPacket packet,
        CancellationToken cancellationToken
    )
    {
        _ = cancellationToken;
        _outgoingPackets.Enqueue(sessionId, packet);

        return Task.CompletedTask;
    }

    private IReadOnlyCollection<long> GetSessionIds()
        => _sessions.GetAll().Select(static session => session.SessionId).ToArray();
}
