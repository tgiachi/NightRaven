using global::DryIoc;
using NightHeaven.Hosting.Interfaces.Services;
using NightHeaven.Server.Extensions.DryIoc;
using NightHeaven.Tests.Support;

namespace NightHeaven.Tests.Hosting;

public class NightHeavenServiceOrchestratorTests
{
    internal sealed class ThrowingStopService : INightHeavenService
    {
        public Task StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
            => throw new InvalidOperationException("boom");
    }

    [Fact]
    public async Task Start_EqualPriorities_PreserveRegistrationOrder()
    {
        var timeline = new List<string>();
        var container = BuildHost(
            timeline,
            ("A", 50),
            ("B", 50),
            ("C", 50)
        );

        await container.Orchestrator().StartAsync(CancellationToken.None);

        Assert.Equal(new[] { "start:A", "start:B", "start:C" }, timeline);
    }

    [Fact]
    public async Task Start_NoServicesRegistered_DoesNotThrow()
    {
        var container = new Container();
        container.AddNightHeavenHosting();

        await container.Orchestrator().StartAsync(CancellationToken.None);
        await container.Orchestrator().StopAsync(CancellationToken.None);
    }

    [Fact]
    public async Task Start_RunsServicesInAscendingPriorityOrder()
    {
        var timeline = new List<string>();
        var container = BuildHost(
            timeline,
            ("A", 30),
            ("B", 10),
            ("C", 20)
        );

        await container.Orchestrator().StartAsync(CancellationToken.None);

        Assert.Equal(new[] { "start:B", "start:C", "start:A" }, timeline);
    }

    [Fact]
    public async Task Stop_ContinuesAfterServiceFailure()
    {
        var timeline = new List<string>();
        var container = new Container();
        container.RegisterInstance(timeline);
        container.AddNightHeavenHosting();
        container.AddNightHeavenService<TestHostingServices.NamedServiceA>(10);
        container.AddNightHeavenService<ThrowingStopService>(20);

        var orchestrator = container.Orchestrator();
        await orchestrator.StartAsync(CancellationToken.None);
        timeline.Clear();
        await orchestrator.StopAsync(CancellationToken.None);

        // ThrowingStopService throws on stop; the orchestrator must still stop NamedServiceA.
        Assert.Contains("stop:A", timeline);
    }

    [Fact]
    public async Task Stop_RunsServicesInReverseStartOrder()
    {
        var timeline = new List<string>();
        var container = BuildHost(
            timeline,
            ("A", 30),
            ("B", 10),
            ("C", 20)
        );

        var orchestrator = container.Orchestrator();
        await orchestrator.StartAsync(CancellationToken.None);
        timeline.Clear();
        await orchestrator.StopAsync(CancellationToken.None);

        Assert.Equal(new[] { "stop:A", "stop:C", "stop:B" }, timeline);
    }

    private static Container BuildHost(List<string> timeline, params (string Name, int Priority)[] services)
    {
        var container = new Container();
        container.RegisterInstance(timeline);
        container.AddNightHeavenHosting();

        foreach (var (name, priority) in services)
        {
            switch (name)
            {
                case "A":
                    container.AddNightHeavenService<TestHostingServices.NamedServiceA>(priority);

                    break;
                case "B":
                    container.AddNightHeavenService<TestHostingServices.NamedServiceB>(priority);

                    break;
                case "C":
                    container.AddNightHeavenService<TestHostingServices.NamedServiceC>(priority);

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(services));
            }
        }

        return container;
    }
}

internal static class TestHostingServices
{
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
