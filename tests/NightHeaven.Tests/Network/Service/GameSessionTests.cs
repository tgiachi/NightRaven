using System.Net.Sockets;
using NightHeaven.Network.Client;
using NightHeaven.Server.Services.Network.Internal;

namespace NightHeaven.Tests.Network.Service;

public class GameSessionTests
{
    [Fact]
    public void SessionId_MatchesOwningClient()
    {
        using var client = NewClient();

        var session = new GameSession(client);

        Assert.Equal(client.SessionId, session.SessionId);
        Assert.Same(client, session.Client);
    }

    [Fact]
    public void WithPendingBytes_PersistsMutationsAcrossCalls()
    {
        using var client = NewClient();
        var session = new GameSession(client);

        session.WithPendingBytes(buffer => buffer.AddRange([1, 2, 3]));

        var observed = Array.Empty<byte>();
        session.WithPendingBytes(buffer => observed = buffer.ToArray());

        Assert.Equal(new byte[] { 1, 2, 3 }, observed);
    }

    [Fact]
    public void WithPendingBytes_NullAction_Throws()
    {
        using var client = NewClient();
        var session = new GameSession(client);

        Assert.Throws<ArgumentNullException>(() => session.WithPendingBytes(null!));
    }

    [Fact]
    public void Ctor_NullClient_Throws()
        => Assert.Throws<ArgumentNullException>(() => new GameSession(null!));

    private static NightHeavenTCPClient NewClient()
        => new(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp));
}
