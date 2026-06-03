using DryIoc;
using Microsoft.Extensions.Hosting;
using NightHeaven.Hosting.Interfaces.Services;
using NightHeaven.Hosting.Internal;

namespace NightHeaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for NightHeaven services and the hosting orchestrator.
/// These replace the MEDI <c>IServiceCollection</c> helpers: ASP.NET Core registers its own
/// services through <c>IServiceCollection</c> (unavoidable in a web host), while every NightHeaven
/// service is registered directly on the DryIoc <see cref="IContainer" />.
/// </summary>
public static class NightHeavenContainerExtensions
{
    /// <summary>
    /// Default service start priority. Lower values start first.
    /// </summary>
    public const int DefaultPriority = 100;

    /// <summary>
    /// Registers the orchestrator that drives start/stop of every <see cref="INightHeavenService" />.
    /// Safe to call multiple times (kept idempotent).
    /// </summary>
    /// <remarks>
    /// The orchestrator is registered as a keyed <see cref="NightHeavenServiceOrchestrator" /> so it
    /// can be resolved and surfaced to the generic host as an <see cref="IHostedService" /> from
    /// <c>IServiceCollection</c> (hosted services are collected from MEDI, not from native DryIoc
    /// registrations). See <c>Program.cs</c> for the bridge registration.
    /// </remarks>
    /// <param name="container">DryIoc container.</param>
    public static IContainer AddNightHeavenHosting(this IContainer container)
    {
        container.Register<NightHeavenServiceOrchestrator>(
            Reuse.Singleton,
            ifAlreadyRegistered: IfAlreadyRegistered.Keep
        );

        return container;
    }

    /// <summary>
    /// Registers an <see cref="INightHeavenService" /> behind an interface alias with a start priority.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    /// <param name="priority">Lower values start first. Default <see cref="DefaultPriority" />.</param>
    public static IContainer AddNightHeavenService<TInterface, TImplementation>(
        this IContainer container,
        int priority = DefaultPriority
    )
        where TInterface : class
        where TImplementation : class, TInterface, INightHeavenService
    {
        container.Register<TImplementation>(Reuse.Singleton);
        container.RegisterMapping<TInterface, TImplementation>();
        container.RegisterDescriptor<TImplementation>(priority);

        return container;
    }

    /// <summary>
    /// Registers an <see cref="INightHeavenService" /> with no public interface alias.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    /// <param name="priority">Lower values start first. Default <see cref="DefaultPriority" />.</param>
    public static IContainer AddNightHeavenService<TImplementation>(
        this IContainer container,
        int priority = DefaultPriority
    )
        where TImplementation : class, INightHeavenService
    {
        container.Register<TImplementation>(Reuse.Singleton);
        container.RegisterDescriptor<TImplementation>(priority);

        return container;
    }

    private static void RegisterDescriptor<TImplementation>(this IContainer container, int priority)
        where TImplementation : class, INightHeavenService
        => container.RegisterDelegate(
            resolver => new NightHeavenServiceDescriptor(resolver.Resolve<TImplementation>(), priority),
            Reuse.Singleton,
            ifAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation,
            serviceKey: typeof(TImplementation)
        );
}
