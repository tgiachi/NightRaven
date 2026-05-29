using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NightHeaven.Hosting.Extensions;
using NightHeaven.Hosting.Interfaces.EventHandlers;
using NightHeaven.Hosting.Interfaces.Events;
using NightHeaven.Hosting.Interfaces.Services;

namespace NightHeaven.Tests.Hosting;

public class NightHeavenServiceOrchestratorTests
{
    [Fact]
    public async Task Start_RunsServicesInAscendingPriorityOrder()
    {
        var timeline = new List<string>();
        var sp = BuildHost(
            timeline,
            ("A", 30),
            ("B", 10),
            ("C", 20)
        );

        var orchestrator = sp.GetRequiredService<IEnumerable<IHostedService>>().Single();
        await orchestrator.StartAsync(CancellationToken.None);

        Assert.Equal(new[] { "start:B", "start:C", "start:A" }, timeline);
    }

    [Fact]
    public async Task Stop_RunsServicesInReverseStartOrder()
    {
        var timeline = new List<string>();
        var sp = BuildHost(
            timeline,
            ("A", 30),
            ("B", 10),
            ("C", 20)
        );

        var orchestrator = sp.GetRequiredService<IEnumerable<IHostedService>>().Single();
        await orchestrator.StartAsync(CancellationToken.None);
        timeline.Clear();
        await orchestrator.StopAsync(CancellationToken.None);

        Assert.Equal(new[] { "stop:A", "stop:C", "stop:B" }, timeline);
    }

    [Fact]
    public async Task Start_EqualPriorities_PreserveRegistrationOrder()
    {
        var timeline = new List<string>();
        var sp = BuildHost(
            timeline,
            ("A", 50),
            ("B", 50),
            ("C", 50)
        );

        var orchestrator = sp.GetRequiredService<IEnumerable<IHostedService>>().Single();
        await orchestrator.StartAsync(CancellationToken.None);

        Assert.Equal(new[] { "start:A", "start:B", "start:C" }, timeline);
    }

    [Fact]
    public async Task Stop_ContinuesAfterServiceFailure()
    {
        var timeline = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton(timeline);
        services.AddNightHeavenHosting();
        services.AddNightHeavenService<TimelineService>(10);
        services.AddNightHeavenService<ThrowingStopService>(20);

        var sp = services.BuildServiceProvider();
        var orchestrator = sp.GetRequiredService<IEnumerable<IHostedService>>().Single();

        await orchestrator.StartAsync(CancellationToken.None);
        timeline.Clear();
        await orchestrator.StopAsync(CancellationToken.None);

        // ThrowingStopService throws on stop; the orchestrator must still stop TimelineService.
        Assert.Contains("stop:Timeline", timeline);
    }

    [Fact]
    public async Task Start_NoServicesRegistered_DoesNotThrow()
    {
        var services = new ServiceCollection();
        services.AddNightHeavenHosting();

        var sp = services.BuildServiceProvider();
        var orchestrator = sp.GetRequiredService<IEnumerable<IHostedService>>().Single();

        await orchestrator.StartAsync(CancellationToken.None);
        await orchestrator.StopAsync(CancellationToken.None);
    }

    private static ServiceProvider BuildHost(List<string> timeline, params (string Name, int Priority)[] services)
    {
        var collection = new ServiceCollection();
        collection.AddSingleton(timeline);
        collection.AddNightHeavenHosting();

        foreach (var (name, priority) in services)
        {
            collection.AddNightHeavenServiceWithName(name, priority);
        }

        return collection.BuildServiceProvider();
    }

    internal sealed class TimelineService : INightHeavenService
    {
        private readonly List<string> _timeline;

        public TimelineService(List<string> timeline)
        {
            _timeline = timeline;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            lock (_timeline)
            {
                _timeline.Add("start:Timeline");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            lock (_timeline)
            {
                _timeline.Add("stop:Timeline");
            }

            return Task.CompletedTask;
        }
    }

    internal sealed class ThrowingStopService : INightHeavenService
    {
        public Task StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
            => throw new InvalidOperationException("boom");
    }
}

internal static class TestHostingExtensions
{
    public static IServiceCollection AddNightHeavenServiceWithName(
        this IServiceCollection services,
        string name,
        int priority
    )
    {
        // Use the typed name registration helper to avoid duplicate type names across tests.
        var serviceType = name switch
        {
            "A" => typeof(NamedServiceA),
            "B" => typeof(NamedServiceB),
            "C" => typeof(NamedServiceC),
            _   => throw new ArgumentOutOfRangeException(nameof(name))
        };

        var method = typeof(ServiceCollectionExtensions)
            .GetMethods()
            .Single(
                m => m.Name == "AddNightHeavenService"
                  && m.IsGenericMethod
                  && m.GetGenericArguments().Length == 1
            );

        method.MakeGenericMethod(serviceType).Invoke(null, [services, priority]);

        return services;
    }

    internal sealed class NamedServiceA : INightHeavenService
    {
        private readonly List<string> _timeline;

        public NamedServiceA(List<string> timeline)
        {
            _timeline = timeline;
        }

        public Task StartAsync(CancellationToken ct)
        {
            lock (_timeline)
            {
                _timeline.Add("start:A");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken ct)
        {
            lock (_timeline)
            {
                _timeline.Add("stop:A");
            }

            return Task.CompletedTask;
        }
    }

    internal sealed class NamedServiceB : INightHeavenService
    {
        private readonly List<string> _timeline;

        public NamedServiceB(List<string> timeline)
        {
            _timeline = timeline;
        }

        public Task StartAsync(CancellationToken ct)
        {
            lock (_timeline)
            {
                _timeline.Add("start:B");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken ct)
        {
            lock (_timeline)
            {
                _timeline.Add("stop:B");
            }

            return Task.CompletedTask;
        }
    }

    internal sealed class NamedServiceC : INightHeavenService
    {
        private readonly List<string> _timeline;

        public NamedServiceC(List<string> timeline)
        {
            _timeline = timeline;
        }

        public Task StartAsync(CancellationToken ct)
        {
            lock (_timeline)
            {
                _timeline.Add("start:C");
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken ct)
        {
            lock (_timeline)
            {
                _timeline.Add("stop:C");
            }

            return Task.CompletedTask;
        }
    }
}
