using DryIoc;
using Microsoft.Extensions.DependencyInjection;
using NightRaven.Core.Extensions.Container;
using NightRaven.Server.Data.Events;
using NightRaven.Server.Services.Seed;
using NightRaven.UO.Domain.Interfaces.Services;
using NightRaven.UO.Domain.Types;
using Serilog;

namespace NightRaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for boot-time seed actions.
/// </summary>
public static class SeedContainerExtensions
{
    extension(IContainer container)
    {
        /// <summary>
        /// Registers the seed service and its <see cref="ServerStartedEvent" /> handler.
        /// </summary>
        public IContainer AddNightRavenSeeds()
        {
            if (!container.IsRegistered<List<SeedAction>>())
            {
                container.RegisterInstance(new List<SeedAction>());
            }

            container.RegisterDelegate<ISeedService>(
                resolver => new SeedService(
                    resolver.Resolve<IServiceProvider>(),
                    resolver.Resolve<List<SeedAction>>()
                ),
                Reuse.Singleton
            );
            container.AddTickEventHandler<SeedServerStartedHandler, ServerStartedEvent>();

            return container;
        }

        /// <summary>
        /// Adds a boot-time seed action.
        /// </summary>
        public IContainer AddSeed(SeedAction action)
        {
            container.AddToRegisterTypedList(action);

            return container;
        }

        /// <summary>
        /// Adds the default first-user seed: admin/admin with administrator level when no users exist.
        /// </summary>
        public IContainer AddDefaultAdminUserSeed()
            => container.AddSeed(
                async (serviceProvider, cancellationToken) =>
                {
                    var log = Log.ForContext(typeof(SeedContainerExtensions));
                    var users = serviceProvider.GetRequiredService<IUserService>();

                    if (await users.CountAsync(cancellationToken) > 0)
                    {
                        return;
                    }

                    await users.CreateAsync(
                        "admin",
                        "admin",
                        UserLevelType.Administrator,
                        isActive: true,
                        cancellationToken
                    );

                    log.Warning(
                        "Added default admin user with username 'admin' and password 'admin' - please change this password immediately!"
                    );
                }
            );
    }
}
