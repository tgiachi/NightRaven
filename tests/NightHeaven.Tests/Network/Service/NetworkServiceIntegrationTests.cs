using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NightHeaven.Hosting.Interfaces.EventHandlers;
using NightHeaven.Network.UO.Registry;
using NightHeaven.Server.Data.Events;
using NightHeaven.Server.Extensions;
using NightHeaven.Server.Interfaces.Network;
using NightHeaven.Server.Services.Network;

namespace NightHeaven.Tests.Network.Service;

public class NetworkServiceIntegrationTests
{
    private sealed class PacketCapture
    {
        public List<PacketReceivedEvent> Packets { get; } = [];
        public List<PlayerConnectedEvent> Connects { get; } = [];
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

    [Fact]
    public async Task FullHost_ClientSendsDoubleClick_HandlerObservesPacketAndConnect()
    {
        var port = GetFreeTcpPort();
        var capture = new PacketCapture();

        var services = new ServiceCollection();
        services.AddSingleton(capture);
        services.AddNightHeavenEventBus();

        var packetRegistry = new PacketRegistry();
        PacketTable.Register(packetRegistry);
        services.AddSingleton(packetRegistry);

        services.AddNightHeavenNetwork(
            cfg =>
            {
                cfg.Port = port;
                cfg.PingServerEnabled = false;
            }
        );
        services.AddTickEventHandler<CapturePacketHandler, PacketReceivedEvent>();
        services.AddTickEventHandler<CaptureConnectHandler, PlayerConnectedEvent>();

        var sp = services.BuildServiceProvider();
        var orchestrator = sp.GetRequiredService<IEnumerable<IHostedService>>().Single();
        var network = (NetworkService)sp.GetRequiredService<INetworkService>();

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
}
