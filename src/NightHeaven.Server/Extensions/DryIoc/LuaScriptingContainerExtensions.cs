using DryIoc;
using NightHeaven.Core.Data.Directories;
using NightHeaven.Core.Types;
using NightHeaven.Core.Utils;
using NightHeaven.Scripting.Lua.Data.Config;
using NightHeaven.Scripting.Lua.Data.Internal;
using NightHeaven.Scripting.Lua.Interfaces;
using NightHeaven.Scripting.Lua.Services;
using NightHeaven.Server.Services.Scripting;

namespace NightHeaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for the NightHeaven Lua scripting engine.
/// </summary>
public static class LuaScriptingContainerExtensions
{
    private const int LuaScriptingPriority = 30;

    /// <summary>
    /// Registers the MoonSharp Lua <see cref="IScriptEngineService" /> and drives its lifecycle
    /// through the NightHeaven hosting orchestrator. The engine resolves the DryIoc
    /// <see cref="IContainer" /> itself to register and resolve script-module types at runtime.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    /// <param name="directoriesConfig">Resolved directories configuration.</param>
    public static IContainer AddNightHeavenLuaScripting(
        this IContainer container,
        DirectoriesConfig directoriesConfig
    )
    {
        ArgumentNullException.ThrowIfNull(directoriesConfig);

        container.AddNightHeavenHosting();

        var scriptsDirectory = directoriesConfig[DirectoryType.Scripts];
        var config = new LuaEngineConfig(scriptsDirectory, scriptsDirectory, VersionUtils.GetVersion());

        container.RegisterInstance(config);
        container.RegisterInstance(directoriesConfig, IfAlreadyRegistered.Keep);

        // Module/userdata accumulator lists the engine resolves from the container.
        container.RegisterInstance(new List<ScriptModuleData>(), IfAlreadyRegistered.Keep);
        container.RegisterInstance(new List<ScriptUserData>(), IfAlreadyRegistered.Keep);

        container.Register<IScriptEngineService, LuaScriptEngineService>(Reuse.Singleton);
        container.AddNightHeavenService<LuaScriptHostedService>(LuaScriptingPriority);

        return container;
    }
}
