using MoonSharp.Interpreter;
using NightRaven.Scripting.Lua.Attributes.Scripts;
using NightRaven.Scripting.Lua.Interfaces.Events;

namespace NightRaven.Scripting.Lua.Modules;

[ScriptModule("events", "Allows Lua scripts to subscribe to named server events.")]
public sealed class EventsModule
{
    private readonly ILuaEventBridge _events;

    public EventsModule(ILuaEventBridge events)
    {
        _events = events;
    }

    [ScriptFunction("on", "Registers a callback for a named server event.")]
    public void On(string eventName, Closure callback)
        => _events.Register(eventName, callback);
}
