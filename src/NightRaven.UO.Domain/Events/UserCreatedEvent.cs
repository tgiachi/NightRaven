using NightRaven.Abstractions.Interfaces.Events;
using NightRaven.Core.Ids;
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
