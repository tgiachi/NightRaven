using NightRaven.Abstractions.Interfaces.EventHandlers;
using NightRaven.Scripting.Lua.Interfaces.Events;
using NightRaven.Server.Data.Events;

namespace NightRaven.Server.Services.Scripting;

public sealed class LuaPlayerConnectedEventHandler : ITickEventHandler<PlayerConnectedEvent>
{
    private readonly ILuaEventBridge _events;

    public LuaPlayerConnectedEventHandler(ILuaEventBridge events)
    {
        _events = events;
    }

    public void Handle(PlayerConnectedEvent evt)
        => _events.Publish(
            "player.connected",
            new Dictionary<string, object?>
            {
                ["session_id"] = evt.SessionId,
                ["remote_endpoint"] = evt.RemoteEndPoint,
                ["at"] = evt.At
            }
        );
}
