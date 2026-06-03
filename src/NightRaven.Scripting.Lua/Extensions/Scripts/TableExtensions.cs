using System.Reflection;
using MoonSharp.Interpreter;
using NightRaven.Scripting.Lua.Proxies;

namespace NightRaven.Scripting.Lua.Extensions.Scripts;

/// <summary>
/// Provides extension methods for MoonSharp Table objects to enable proxying to interfaces.
/// </summary>
public static class TableExtensions
{
    /// <summary>
    /// Converts a MoonSharp Table to a proxy implementing the specified interface.
    /// </summary>
    public static TInterface ToProxy<TInterface>(this Table table)
        where TInterface : class
    {
        var proxy = DispatchProxy.Create<TInterface, LuaProxy<TInterface>>();
        ((LuaProxy<TInterface>)(object)proxy).Table = table;

        return proxy;
    }
}
