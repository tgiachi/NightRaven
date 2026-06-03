using System.Net.Sockets;
using NightRaven.Network.Client;
using NightRaven.Server.Services.Network;

namespace NightRaven.Tests.Network.Service;

public class SessionServiceTests
{
    [Fact]
    public void Clear_RemovesEverySession()
    {
        var service = new SessionService();
        using var a = NewClient();
        using var b = NewClient();
        service.GetOrCreate(a);
        service.GetOrCreate(b);

        service.Clear();

        Assert.Equal(0, service.Count);
        Assert.Empty(service.GetAll());
    }

    [Fact]
    public void GetOrCreate_SameClient_ReturnsSameSession()
    {
        var service = new SessionService();
        using var client = NewClient();

        var first = service.GetOrCreate(client);
        var second = service.GetOrCreate(client);

        Assert.Same(first, second);
        Assert.Equal(1, service.Count);
        Assert.Equal(client.SessionId, first.SessionId);
    }

    [Fact]
    public void Remove_ExistingSession_ReturnsTrueAndDecrementsCount()
    {
        var service = new SessionService();
        using var client = NewClient();
        service.GetOrCreate(client);

        Assert.True(service.Remove(client.SessionId));
        Assert.Equal(0, service.Count);
        Assert.False(service.Remove(client.SessionId));
    }

    [Fact]
    public void TryGet_AfterCreate_ReturnsSession()
    {
        var service = new SessionService();
        using var client = NewClient();
        var created = service.GetOrCreate(client);

        Assert.True(service.TryGet(client.SessionId, out var found));
        Assert.Same(created, found);
    }

    [Fact]
    public void TryGet_UnknownId_ReturnsFalse()
    {
        var service = new SessionService();

        Assert.False(service.TryGet(12345, out _));
    }

    private static NightRavenTCPClient NewClient()
        => new(new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
}
