using System.Net;
using System.Net.Sockets;
using DryIoc;
using NightRaven.Hosting.Interfaces.EventHandlers;
using NightRaven.Network.UO.Registry;
using NightRaven.Server.Data.Events;
using NightRaven.Server.Extensions.DryIoc;
using NightRaven.Server.Interfaces.Network;
using NightRaven.Server.Services.Network;
using NightRaven.Tests.Support;

namespace NightRaven.Tests.Network.Service;

public class NetworkServiceIntegrationTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nh-network-config-{Guid.NewGuid():N}");

    private sealed class PacketCapture
    {
        public List<PacketReceivedEvent> Packets { get; } = [];
        public List<PlayerConnectedEvent> Connects { get; } = [];
        public List<PlayerDisconnectedEvent> Disconnects { get; } = [];
    }

    private sealed class CapturePacketHandler : ITickEventHandler<PacketReceivedEvent>
    {
        private readonly PacketCapture _capture;

        public CapturePacketHandler(PacketCapture capture)
        {
            _capture = capture;
        }

        public void Handle(PacketReceivedEvent evt)
        {
            lock (_capture)
            {
                _capture.Packets.Add(evt);
            }
        }
    }

    private sealed class CaptureConnectHandler : ITickEventHandler<PlayerConnectedEvent>
    {
        private readonly PacketCapture _capture;

        public CaptureConnectHandler(PacketCapture capture)
        {
            _capture = capture;
        }

        public void Handle(PlayerConnectedEvent evt)
        {
            lock (_capture)
            {
                _capture.Connects.Add(evt);
            }
        }
    }

    private sealed class CaptureDisconnectHandler : ITickEventHandler<PlayerDisconnectedEvent>
    {
        private readonly PacketCapture _capture;

        public CaptureDisconnectHandler(PacketCapture capture)
        {
            _capture = capture;
        }

        public void Handle(PlayerDisconnectedEvent evt)
        {
            lock (_capture)
            {
                _capture.Disconnects.Add(evt);
            }
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task FullHost_ClientDisconnects_PublishesDisconnectAndRemovesSession()
    {
        var port = GetFreeTcpPort();
        var capture = new PacketCapture();
        var configPath = WriteNetworkConfig(port);

        var container = new Container();
        container.RegisterInstance(capture);
        container.AddNightRavenEventBus();

        var packetRegistry = new PacketRegistry();
        PacketTable.Register(packetRegistry);
        container.RegisterInstance(packetRegistry);

        container.AddNightRavenNetwork();
        container.AddNightRavenConfig(configPath);
        container.AddTickEventHandler<CaptureDisconnectHandler, PlayerDisconnectedEvent>();

        var orchestrator = container.Orchestrator();
        var network = (NetworkService)container.Resolve<INetworkService>();

        await orchestrator.StartAsync(CancellationToken.None);

        try
        {
            var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, port);

            await WaitForAsync(() => network.ConnectedSessionCount >= 1, TimeSpan.FromSeconds(5));

            client.Close();
            client.Dispose();

            await WaitForAsync(
                () =>
                {
                    lock (capture)
                    {
                        return capture.Disconnects.Count >= 1;
                    }
                },
                TimeSpan.FromSeconds(5)
            );

            lock (capture)
            {
                Assert.Single(capture.Disconnects);
            }

            await WaitForAsync(() => network.ConnectedSessionCount == 0, TimeSpan.FromSeconds(5));
            Assert.Equal(0, network.ConnectedSessionCount);
        }
        finally
        {
            await orchestrator.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task FullHost_ClientSendsDoubleClick_HandlerObservesPacketAndConnect()
    {
        var port = GetFreeTcpPort();
        var capture = new PacketCapture();
        var configPath = WriteNetworkConfig(port);

        var container = new Container();
        container.RegisterInstance(capture);
        container.AddNightRavenEventBus();

        var packetRegistry = new PacketRegistry();
        PacketTable.Register(packetRegistry);
        container.RegisterInstance(packetRegistry);

        container.AddNightRavenNetwork();
        container.AddNightRavenConfig(configPath);
        container.AddTickEventHandler<CapturePacketHandler, PacketReceivedEvent>();
        container.AddTickEventHandler<CaptureConnectHandler, PlayerConnectedEvent>();

        var orchestrator = container.Orchestrator();
        var network = (NetworkService)container.Resolve<INetworkService>();

        await orchestrator.StartAsync(CancellationToken.None);

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, port);

            // DoubleClick packet: opcode 0x06 + 4-byte serial (fixed length 5).
            await client.GetStream().WriteAsync(new byte[] { 0x06, 0x00, 0x00, 0x00, 0x2A });

            await WaitForAsync(
                () =>
                {
                    lock (capture)
                    {
                        return capture.Packets.Count >= 1 && capture.Connects.Count >= 1;
                    }
                },
                TimeSpan.FromSeconds(5)
            );

            lock (capture)
            {
                Assert.Single(capture.Packets);
                Assert.Equal(0x06, capture.Packets[0].OpCode);
                Assert.Single(capture.Connects);
            }

            Assert.True(network.ConnectedSessionCount >= 1);

            var samples = network.Collect().ToDictionary(s => s.Name, s => s.Value);
            Assert.True(samples["active_sessions"] >= 1);
            Assert.True(samples["parsed_packets_total"] >= 1);
        }
        finally
        {
            await orchestrator.StopAsync(CancellationToken.None);
        }
    }

    private static int GetFreeTcpPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        return port;
    }

    private static async Task WaitForAsync(Func<bool> condition, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(20);
        }

        throw new TimeoutException($"Condition not met within {timeout}.");
    }

    private string WriteNetworkConfig(int port)
    {
        Directory.CreateDirectory(_dir);
        var path = Path.Combine(_dir, $"network-{port}.toml");
        File.WriteAllText(path, $"[network]\nport = {port}\nping_server_enabled = false\n");

        return path;
    }
}
