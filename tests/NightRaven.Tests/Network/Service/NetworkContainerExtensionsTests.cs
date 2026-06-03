using DryIoc;
using NightRaven.Network.UO.Registry;
using NightRaven.Server.Extensions.DryIoc;
using NightRaven.Server.Interfaces.Network;

namespace NightRaven.Tests.Network.Service;

public class NetworkContainerExtensionsTests : IDisposable
{
    private readonly IContainer _container = new Container();

    public void Dispose()
    {
        _container.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void AddNightRavenNetwork_RegistersOutgoingPacketQueue()
    {
        _container.RegisterInstance(new PacketRegistry());

        _container.AddNightRavenNetwork();

        Assert.NotNull(_container.Resolve<IOutgoingPacketQueue>());
    }
}
