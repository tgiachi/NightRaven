using DryIoc;
using NightRaven.Core.Data.Directories;
using NightRaven.Core.Types;
using NightRaven.Core.Utils;
using NightRaven.Scripting.Lua.Data.Config;
using NightRaven.Scripting.Lua.Data.Internal;
using NightRaven.Scripting.Lua.Interfaces;
using NightRaven.Scripting.Lua.Services;
using NightRaven.Server.Services.Scripting;

namespace NightRaven.Server.Extensions.DryIoc;

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

        container.Register<IScriptEngineService, LuaScriptEngineService>(Reuse.Singleton);
        container.AddNightRavenService<LuaScriptHostedService>(LuaScriptingPriority);

        return container;
    }
}
