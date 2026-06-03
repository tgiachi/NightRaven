using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NightHeaven.Core.Data.Directories;
using NightHeaven.Scripting.Lua.Interfaces;
using NightHeaven.Server.Extensions;

namespace NightHeaven.Tests.Scripting.Lua;

public class LuaScriptingIntegrationTests
{
    [Fact]
    public async Task FullHost_OnDryIoc_StartsEngineAndRunsBootstrapScript()
    {
        var scriptsDir = Path.Combine(Path.GetTempPath(), $"nh-lua-int-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(scriptsDir, "scripts"));

        try
        {
            // A bootstrap.lua that writes a marker file proves the engine ran user scripts.
            var markerPath = Path.Combine(scriptsDir, "ran.txt");
            var luaMarker = markerPath.Replace("\\", "\\\\");
            await File.WriteAllTextAsync(
                Path.Combine(scriptsDir, "scripts", "bootstrap.lua"),
                $"local f = io.open(\"{luaMarker}\", \"w\"); f:write(\"ok\"); f:close()"
            );

            var directoriesConfig = new DirectoriesConfig(scriptsDir, Array.Empty<string>());

            var services = new ServiceCollection();
            services.AddNightHeavenLuaScripting(directoriesConfig);

            // Back the provider with DryIoc, exactly like the host does, so the engine can
            // resolve DryIoc.IContainer at runtime.
            var provider = new DryIocServiceProviderFactory()
                           .CreateBuilder(services)
                           .BuildServiceProvider();

            var orchestrator = provider.GetRequiredService<IEnumerable<IHostedService>>().Single();
            var engine = provider.GetRequiredService<IScriptEngineService>();

            await orchestrator.StartAsync(CancellationToken.None);

            try
            {
                Assert.True(File.Exists(markerPath), "bootstrap.lua should have run and created the marker file");

                // Engine is live: a function call returns a result.
                var result = engine.ExecuteFunction("1 + 1");
                Assert.True(result.Success);
                Assert.Equal(2d, Assert.IsType<double>(result.Data));
            }
            finally
            {
                await orchestrator.StopAsync(CancellationToken.None);
            }
        }
        finally
        {
            if (Directory.Exists(scriptsDir))
            {
                Directory.Delete(scriptsDir, true);
            }
        }
    }

    [Fact]
    public void AddNightHeavenLuaScripting_RegistersEngineAndHostedService()
    {
        var scriptsDir = Path.Combine(Path.GetTempPath(), $"nh-lua-reg-{Guid.NewGuid():N}");
        Directory.CreateDirectory(scriptsDir);

        try
        {
            var directoriesConfig = new DirectoriesConfig(scriptsDir, Array.Empty<string>());

            var services = new ServiceCollection();
            services.AddNightHeavenLuaScripting(directoriesConfig);

            var provider = new DryIocServiceProviderFactory()
                           .CreateBuilder(services)
                           .BuildServiceProvider();

            Assert.NotNull(provider.GetService<IScriptEngineService>());
            Assert.NotEmpty(provider.GetServices<IHostedService>());
        }
        finally
        {
            if (Directory.Exists(scriptsDir))
            {
                Directory.Delete(scriptsDir, true);
            }
        }
    }
}
