namespace NightRaven.Network.UO.Packets.Incoming.Login;

/// <summary>
/// Numeric UO client version carried by the login seed packet.
/// </summary>
public readonly record struct ClientVersion
{
    public int Major { get; }
    public int Minor { get; }
    public int Revision { get; }
    public int Patch { get; }

    public ClientVersion(int major, int minor, int revision, int patch)
    {
        Major = major;
        Minor = minor;
        Revision = revision;
        Patch = patch;
    }
}
