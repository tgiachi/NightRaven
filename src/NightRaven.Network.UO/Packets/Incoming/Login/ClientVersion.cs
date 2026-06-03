namespace NightRaven.Network.UO.Packets.Incoming.Login;

/// <summary>
/// Numeric UO client version carried by the login seed packet.
/// </summary>
public readonly record struct ClientVersion(int Major, int Minor, int Revision, int Patch);
