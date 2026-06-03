using DryIoc;
using NightRaven.Hosting.Interfaces.Services;
using NightRaven.Hosting.Internal;

namespace NightRaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for NightRaven services and the hosting orchestrator.
/// These replace the MEDI <c>IServiceCollection</c> helpers: ASP.NET Core registers its own
/// services through <c>IServiceCollection</c> (unavoidable in a web host), while every NightRaven
/// service is registered directly on the DryIoc <see cref="IContainer" />.
/// </summary>
public static class NightRavenContainerExtensions
{
    /// <summary>
    /// Default service start priority. Lower values start first.
    /// </summary>
    public const int DefaultPriority = 100;

    /// <summary>
    /// Registers the orchestrator that drives start/stop of every <see cref="INightRavenService" />.
    /// Safe to call multiple times (kept idempotent).
    /// </summary>
    /// <remarks>
    /// The orchestrator is registered as a keyed <see cref="NightRavenServiceOrchestrator" /> so it
    /// can be resolved and surfaced to the generic host as an <see cref="IHostedService" /> from
    /// <c>IServiceCollection</c> (hosted services are collected from MEDI, not from native DryIoc
    /// registrations). See <c>Program.cs</c> for the bridge registration.
    /// </remarks>
    /// <param name="container">DryIoc container.</param>
    public static IContainer AddNightRavenHosting(this IContainer container)
    {
        container.Register<NightRavenServiceOrchestrator>(
            Reuse.Singleton,
            ifAlreadyRegistered: IfAlreadyRegistered.Keep
        );

        return container;
    }

    /// <summary>
    /// Registers an <see cref="INightRavenService" /> behind an interface alias with a start priority.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    /// <param name="priority">Lower values start first. Default <see cref="DefaultPriority" />.</param>
    public static IContainer AddNightRavenService<TInterface, TImplementation>(
        this IContainer container,
        int priority = DefaultPriority
    )
        where TInterface : class
        where TImplementation : class, TInterface, INightRavenService
    {
        container.Register<TImplementation>(Reuse.Singleton);
        container.RegisterMapping<TInterface, TImplementation>();
        container.RegisterDescriptor<TImplementation>(priority);

        return container;
    }

    /// <summary>
    /// Registers an <see cref="INightRavenService" /> with no public interface alias.
    /// </summary>
    /// <param name="container">DryIoc container.</param>
    /// <param name="priority">Lower values start first. Default <see cref="DefaultPriority" />.</param>
    public static IContainer AddNightRavenService<TImplementation>(
        this IContainer container,
        int priority = DefaultPriority
    )
        where TImplementation : class, INightRavenService
    {
        container.Register<TImplementation>(Reuse.Singleton);
        container.RegisterDescriptor<TImplementation>(priority);

        return container;
    }

    private static void RegisterDescriptor<TImplementation>(this IContainer container, int priority)
        where TImplementation : class, INightRavenService
        => container.RegisterDelegate(
            resolver => new NightRavenServiceDescriptor(resolver.Resolve<TImplementation>(), priority),
            Reuse.Singleton,
            ifAlreadyRegistered: IfAlreadyRegistered.AppendNewImplementation,
            serviceKey: typeof(TImplementation)
        );
}
