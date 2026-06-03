using Microsoft.Extensions.DependencyInjection.Extensions;
using NightHeaven.Core.Data.Directories;
using NightHeaven.Core.Types;
using NightHeaven.Core.Utils;
using NightHeaven.Hosting.Extensions;
using NightHeaven.Scripting.Lua.Data.Config;
using NightHeaven.Scripting.Lua.Data.Internal;
using NightHeaven.Scripting.Lua.Interfaces;
using NightHeaven.Scripting.Lua.Services;
using NightHeaven.Server.Services.Scripting;

namespace NightHeaven.Server.Extensions;

/// <summary>
/// DI registration helpers for the NightHeaven Lua scripting engine.
/// </summary>
public static class LuaScriptingServiceCollectionExtensions
{
    private const int LuaScriptingPriority = 30;

    /// <summary>
    /// Registers the MoonSharp Lua <see cref="IScriptEngineService" /> and drives its lifecycle through
    /// the NightHeaven hosting orchestrator. Requires the host to be backed by DryIoc (see
    /// <c>UseServiceProviderFactory(new DryIocServiceProviderFactory())</c>) so the engine can register
    /// and resolve script-module types at runtime.
    /// </summary>
    /// <param name="services">DI service collection.</param>
    /// <param name="directoriesConfig">Resolved directories configuration.</param>
    public static IServiceCollection AddNightHeavenLuaScripting(
        this IServiceCollection services,
        DirectoriesConfig directoriesConfig
    )
    {
        ArgumentNullException.ThrowIfNull(directoriesConfig);

        services.AddNightHeavenHosting();

        var scriptsDirectory = directoriesConfig[DirectoryType.Scripts];
        var config = new LuaEngineConfig(scriptsDirectory, scriptsDirectory, VersionUtils.GetVersion());
        services.AddSingleton(config);
        services.AddSingleton(directoriesConfig);

        // Module/userdata accumulator lists the engine resolves from the container.
        services.TryAddSingleton(new List<ScriptModuleData>());
        services.TryAddSingleton(new List<ScriptUserData>());

        services.AddSingleton<IScriptEngineService, LuaScriptEngineService>();
        services.AddNightHeavenService<LuaScriptHostedService>(LuaScriptingPriority);

        return services;
    }
}
