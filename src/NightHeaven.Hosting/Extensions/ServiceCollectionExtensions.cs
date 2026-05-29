using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NightHeaven.Hosting.Interfaces;
using NightHeaven.Hosting.Internal;

namespace NightHeaven.Hosting.Extensions;

/// <summary>
/// Extensions to register NightHeaven services and the hosting orchestrator.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Default service start priority. Lower values start first.
    /// </summary>
    public const int DefaultPriority = 100;

    /// <param name="services">DI service collection.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the orchestrator that drives start/stop of every
        /// <see cref="INightHeavenService" /> registered via
        /// <see cref="AddNightHeavenService{TInterface,TImplementation}" /> or
        /// <see cref="AddNightHeavenService{TImplementation}" />.
        /// Safe to call multiple times.
        /// </summary>
        public IServiceCollection AddNightHeavenHosting()
        {
            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<Microsoft.Extensions.Hosting.IHostedService, NightHeavenServiceOrchestrator>()
            );

            return services;
        }

        /// <summary>
        /// Registers an <see cref="INightHeavenService" /> behind an interface alias
        /// with the given start priority.
        /// </summary>
        /// <param name="priority">Lower values start first. Default <see cref="DefaultPriority" />.</param>
        public IServiceCollection AddNightHeavenService<TInterface, TImplementation>(
            int priority = DefaultPriority
        )
            where TInterface : class
            where TImplementation : class, TInterface, INightHeavenService
        {
            services.AddSingleton<TImplementation>();
            services.AddSingleton<TInterface>(sp => sp.GetRequiredService<TImplementation>());
            services.AddSingleton(
                sp => new NightHeavenServiceDescriptor(sp.GetRequiredService<TImplementation>(), priority)
            );

            return services;
        }

        /// <summary>
        /// Registers an <see cref="INightHeavenService" /> with no public interface alias.
        /// </summary>
        /// <param name="priority">Lower values start first. Default <see cref="DefaultPriority" />.</param>
        public IServiceCollection AddNightHeavenService<TImplementation>(
            int priority = DefaultPriority
        )
            where TImplementation : class, INightHeavenService
        {
            services.AddSingleton<TImplementation>();
            services.AddSingleton(
                sp => new NightHeavenServiceDescriptor(sp.GetRequiredService<TImplementation>(), priority)
            );

            return services;
        }
    }
}
