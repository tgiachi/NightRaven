using NightHeaven.Hosting.Interfaces.Services;
using NightHeaven.Scripting.Lua.Interfaces;

namespace NightHeaven.Server.Services.Scripting;

/// <summary>
/// Hosting adapter that drives the Lua <see cref="IScriptEngineService" /> lifecycle through the
/// NightHeaven service orchestrator. Keeps the scripting project free of any hosting dependency.
/// </summary>
public sealed class LuaScriptHostedService : INightHeavenService
{
    private readonly IScriptEngineService _engine;

    public LuaScriptHostedService(IScriptEngineService engine)
    {
        _engine = engine;
    }

    public Task StartAsync(CancellationToken cancellationToken)
        => _engine.StartAsync();

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_engine is IDisposable disposable)
        {
            disposable.Dispose();
        }

        return Task.CompletedTask;
    }
}
