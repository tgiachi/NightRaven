using NightRaven.Abstractions.Interfaces.EventHandlers;
using NightRaven.Scripting.Lua.Interfaces.Events;
using NightRaven.Server.Data.Events;

namespace NightRaven.Server.Services.Scripting;

public sealed class LuaPlayerDisconnectedEventHandler : ITickEventHandler<PlayerDisconnectedEvent>
{
    private readonly ILuaEventBridge _events;

    public LuaPlayerDisconnectedEventHandler(ILuaEventBridge events)
    {
        _events = events;
    }

    public void Handle(PlayerDisconnectedEvent evt)
        => _events.Publish(
            "player.disconnected",
            new Dictionary<string, object?>
            {
                ["session_id"] = evt.SessionId,
                ["remote_endpoint"] = evt.RemoteEndPoint,
                ["at"] = evt.At
            }
        );
}
