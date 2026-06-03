using NightRaven.Hosting.Interfaces.Services;
using NightRaven.Scripting.Lua.Interfaces;

namespace NightRaven.Server.Services.Scripting;

/// <summary>
/// Hosting adapter that drives the Lua <see cref="IScriptEngineService" /> lifecycle through the
/// NightRaven service orchestrator. Keeps the scripting project free of any hosting dependency.
/// </summary>
public sealed class LuaScriptHostedService : INightRavenService
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
