using NightRaven.Abstractions.Interfaces.EventHandlers;
using NightRaven.Scripting.Lua.Interfaces.Events;
using NightRaven.Server.Data.Events;

namespace NightRaven.Server.Services.Scripting;

public sealed class LuaServerStartedEventHandler : ITickEventHandler<ServerStartedEvent>
{
    private readonly ILuaEventBridge _events;

    public LuaServerStartedEventHandler(ILuaEventBridge events)
    {
        _events = events;
    }

    public void Handle(ServerStartedEvent evt)
        => _events.Publish(
            "server.started",
            new Dictionary<string, object?>
            {
                ["at"] = evt.At
            }
        );
}
