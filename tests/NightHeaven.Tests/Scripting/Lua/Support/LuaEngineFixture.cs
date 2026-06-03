using DryIoc;
using NightHeaven.Core.Data.Directories;
using NightHeaven.Scripting.Lua.Data.Config;
using NightHeaven.Scripting.Lua.Data.Internal;
using NightHeaven.Scripting.Lua.Services;

namespace NightHeaven.Tests.Scripting.Lua.Support;

/// <summary>
/// Builds a real <see cref="LuaScriptEngineService" /> backed by a throwaway temp scripts directory
/// and a DryIoc container, and cleans both up on dispose. Used by the Lua engine unit tests.
/// </summary>
internal sealed class LuaEngineFixture : IDisposable
{
    private readonly string _scriptsDirectory;
    private readonly Container _container;

    public LuaEngineFixture(IEnumerable<ScriptModuleData>? modules = null)
    {
        _scriptsDirectory = Path.Combine(Path.GetTempPath(), $"nh-lua-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_scriptsDirectory);

        _container = new Container();

        var directoriesConfig = new DirectoriesConfig(_scriptsDirectory, Array.Empty<string>());
        var config = new LuaEngineConfig(_scriptsDirectory, _scriptsDirectory, "test");

        Engine = new(
            directoriesConfig,
            _container,
            config,
            modules?.ToList() ?? [],
            []
        );
    }

    public LuaScriptEngineService Engine { get; }

    public string ScriptsDirectory => _scriptsDirectory;

    public void Dispose()
    {
        Engine.Dispose();
        _container.Dispose();

        if (Directory.Exists(_scriptsDirectory))
        {
            Directory.Delete(_scriptsDirectory, true);
        }
    }
}
