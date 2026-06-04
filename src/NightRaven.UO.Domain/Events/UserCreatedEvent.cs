using NightRaven.Abstractions.Interfaces.Events;
using NightRaven.Core.Ids;
using NightRaven.UO.Domain.Types;

namespace NightRaven.UO.Domain.Events;

/// <summary>
/// Async event published after a user account has been persisted.
/// </summary>
public sealed record UserCreatedEvent : IAsyncEvent
{
    public Serial UserId { get; }
    public string Username { get; }
    public UserLevelType Level { get; }
    public bool IsActive { get; }
    public DateTimeOffset At { get; }

    public UserCreatedEvent(
        Serial userId,
        string username,
        UserLevelType level,
        bool isActive,
        DateTimeOffset at
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        UserId = userId;
        Username = username;
        Level = level;
        IsActive = isActive;
        At = at;
    }
}
