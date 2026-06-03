using DryIoc;
using NightHeaven.Core.Extensions.Container;
using NightHeaven.Hosting.Data.Persistence;
using NightHeaven.Hosting.Interfaces.Metrics;
using NightHeaven.Hosting.Internal;
using NightHeaven.Persistence.Data;
using NightHeaven.Persistence.Interfaces.Persistence;
using NightHeaven.Persistence.Services.Persistence;

namespace NightHeaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for the NightHeaven persistence engine.
/// </summary>
public static class PersistenceContainerExtensions
{
    private const int PersistencePriority = 15;

    /// <param name="container">DryIoc container.</param>
    extension(IContainer container)
    {
        /// <summary>
        /// Registers a persisted entity type. Accumulates a descriptor consumed by the persistence
        /// service at boot. Call before <see cref="AddNightHeavenPersistence" />'s service starts.
        /// </summary>
        /// <param name="typeId">Stable numeric identifier for the entity kind.</param>
        /// <param name="schemaVersion">Version of the persisted entity schema.</param>
        /// <param name="keySelector">Selects the entity key.</param>
        public IContainer RegisterPersistenceEntity<TEntity, TKey>(
            ushort typeId,
            int schemaVersion,
            Func<TEntity, TKey> keySelector
        )
            where TKey : notnull
        {
            var descriptor = new PersistenceEntityDescriptor<TEntity, TKey>(
                typeId,
                typeof(TEntity).Name,
                schemaVersion,
                keySelector
            );
            container.AddToRegisterTypedList(new PersistenceEntityRegistration(descriptor));

            return container;
        }

        /// <summary>
        /// Registers the persistence service (snapshot + journal) with the hosting orchestrator and the
        /// open-generic <see cref="IDataAccess{TEntity,TKey}" />.
        /// </summary>
        /// <param name="saveDirectory">Directory for snapshot/journal files.</param>
        public IContainer AddNightHeavenPersistence(string saveDirectory)
        {
            container.AddNightHeavenHosting();

            container.RegisterConfigSection<PersistenceConfig>("persistence", () => new PersistenceConfig());

            // Ensure a (possibly empty) registration list exists even when no entity was registered.
            if (!container.IsRegistered<List<PersistenceEntityRegistration>>())
            {
                container.RegisterInstance(new List<PersistenceEntityRegistration>());
            }

            // The service ctor takes the save directory + config + accumulated registrations, so build it
            // through a delegate. Register it behind its interface only: the host container (MS DI rules)
            // produces duplicate factories for RegisterDelegate, and the strict RegisterMapping rejects
            // multiple factories — resolving through the interface uses last-registered and stays safe.
            container.RegisterDelegate<IPersistenceService>(
                resolver => new PersistenceService(
                    saveDirectory,
                    resolver.Resolve<PersistenceConfig>(),
                    resolver.Resolve<List<PersistenceEntityRegistration>>()
                ),
                Reuse.Singleton
            );

            // Drive start/stop through the orchestrator at priority 15 (after TimerWheel=3, before Network=20).
            container.RegisterDelegate(
                resolver => new NightHeavenServiceDescriptor(resolver.Resolve<IPersistenceService>(), PersistencePriority),
                Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation,
                serviceKey: typeof(PersistenceService)
            );

            // Surface persistence metrics alongside the other providers (the singleton is an IMetricProvider).
            container.RegisterDelegate<IMetricProvider>(
                resolver => (IMetricProvider)resolver.Resolve<IPersistenceService>(),
                Reuse.Singleton,
                ifAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation
            );

            // Open-generic IDataAccess<,> resolves through the service's GetDataAccess factory method.
            container.Register(
                typeof(IDataAccess<,>),
                made: Made.Of(
                    request => typeof(IPersistenceService).GetMethod(nameof(IPersistenceService.GetDataAccess))!
                                                          .MakeGenericMethod(request.ServiceType.GetGenericArguments()),
                    ServiceInfo.Of<IPersistenceService>()
                ),
                setup: Setup.With(asResolutionCall: true)
            );

            return container;
        }
    }
}
