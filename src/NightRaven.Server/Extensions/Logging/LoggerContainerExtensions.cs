using DryIoc;
using NightRaven.Abstractions.Data.Logging;
using NightRaven.Abstractions.Extensions.DryIoc;

namespace NightRaven.Server.Extensions.Logging;

/// <summary>
/// DryIoc-native registration helpers for logging configuration.
/// </summary>
public static class LoggerContainerExtensions
{
    extension(IContainer container)
    {
        /// <summary>
        /// Registers the logger TOML config section.
        /// </summary>
        public IContainer AddNightRavenLogging()
        {
            container.RegisterConfigSection("logger", () => new LoggerConfig());

            return container;
        }
    }
}
