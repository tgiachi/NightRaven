using NightRaven.Abstractions.Types.Player;
using NightRaven.Core.Ids;

namespace NightRaven.Abstractions.Data.Player;

/// <summary>
/// Logical player session state associated with a network session.
/// </summary>
public sealed class PlayerSession
{
    public long SessionId { get; set; }
    public string? RemoteEndPoint { get; set; }
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public Serial? CharacterSerial { get; set; }
    public Serial? MobileSerial { get; set; }
    public PlayerSessionStateType State { get; set; } = PlayerSessionStateType.Connected;
    public DateTimeOffset ConnectedAt { get; set; }
    public DateTimeOffset? AuthenticatedAt { get; set; }
    public DateTimeOffset? EnteredWorldAt { get; set; }
    public DateTimeOffset? DisconnectedAt { get; set; }
    public byte? ViewRange { get; set; }
    public string? ClientVersion { get; set; }
}
