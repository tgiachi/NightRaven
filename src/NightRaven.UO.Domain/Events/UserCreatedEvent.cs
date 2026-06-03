using NightRaven.Core.Ids;
using NightRaven.Hosting.Interfaces.Events;
using NightRaven.UO.Domain.Types;

namespace NightRaven.UO.Domain.Events;

/// <summary>
/// Async event published after a user account has been persisted.
/// </summary>
public sealed record UserCreatedEvent(
    Serial UserId,
    string Username,
    UserLevelType Level,
    bool IsActive,
    DateTimeOffset At
) : IAsyncEvent;
