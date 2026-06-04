using DryIoc;
using NightRaven.Abstractions.Extensions.DryIoc;
using NightRaven.Core.Data.Directories;
using NightRaven.Core.Types;
using NightRaven.Core.Utils;
using NightRaven.Scripting.Lua.Data.Config;
using NightRaven.Scripting.Lua.Data.Internal;
using NightRaven.Scripting.Lua.Extensions.Scripts;
using NightRaven.Scripting.Lua.Interfaces.Events;
using NightRaven.Scripting.Lua.Interfaces.Scripts;
using NightRaven.Scripting.Lua.Modules;
using NightRaven.Scripting.Lua.Services;
using NightRaven.Server.Data.Events;
using NightRaven.Server.Extensions.Hosting;
using NightRaven.Server.Services.Scripting;

namespace NightRaven.Server.Extensions.Scripting;

/// <summary>
/// DryIoc-native registration helpers for the NightRaven Lua scripting engine.
/// </summary>
public static class LuaScriptingContainerExtensions
{
    private const int LuaScriptingPriority = 30;

    /// <summary>
    /// Registers the MoonSharp Lua <see cref="IScriptEngineService" /> and drives its lifecycle
    /// through the NightRaven hosting orchestrator. The engine resolves the DryIoc
    /// <see cref="IContainer" /> itself to register and resolve script-module types at runtime.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    /// <param name="directoriesConfig">Resolved directories configuration.</param>
    public static IContainer AddNightRavenLuaScripting(
        this IContainer container,
        DirectoriesConfig directoriesConfig
    )
    {
        ArgumentNullException.ThrowIfNull(directoriesConfig);

        container.AddNightRavenHosting();

        var scriptsDirectory = directoriesConfig[DirectoryType.Scripts];
        var config = new LuaEngineConfig(scriptsDirectory, scriptsDirectory, VersionUtils.GetVersion());

        container.RegisterInstance(config);
        container.RegisterInstance(directoriesConfig, IfAlreadyRegistered.Keep);

        // Module/userdata accumulator lists the engine resolves from the container.
        container.RegisterInstance(new List<ScriptModuleData>(), IfAlreadyRegistered.Keep);
        container.RegisterInstance(new List<ScriptUserData>(), IfAlreadyRegistered.Keep);

        container.Register<ILuaEventBridge, LuaEventBridge>(Reuse.Singleton);

        container.Register<IScriptEngineService, LuaScriptEngineService>(Reuse.Singleton);
        container.AddNightRavenService<LuaScriptHostedService>(LuaScriptingPriority);

        container.RegisterScriptModule<EventsModule>();
        container.RegisterScriptModule<LogModule>();
        container.RegisterScriptModule<RandomModule>();
        container.RegisterScriptModule<TimersModule>();

        container.AddTickEventHandler<LuaServerStartedEventHandler, ServerStartedEvent>();
        container.AddTickEventHandler<LuaPlayerConnectedEventHandler, PlayerConnectedEvent>();
        container.AddTickEventHandler<LuaPlayerDisconnectedEventHandler, PlayerDisconnectedEvent>();

        return container;
    }
}
