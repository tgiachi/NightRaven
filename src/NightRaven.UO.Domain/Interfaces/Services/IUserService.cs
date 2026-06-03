using NightRaven.Core.Ids;
using NightRaven.UO.Domain.Entities;
using NightRaven.UO.Domain.Types;

namespace NightRaven.UO.Domain.Interfaces.Services;

/// <summary>
/// Provides account-level access to UO users for the server and plugins.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Creates a user with an auto-allocated serial and a hashed password.
    /// </summary>
    ValueTask<UserEntity> CreateAsync(
        string username,
        string password,
        UserLevelType level = UserLevelType.Player,
        bool isActive = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>Returns the current number of persisted users.</summary>
    ValueTask<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a user by serial, or null when absent.</summary>
    ValueTask<UserEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default);

    /// <summary>Gets a user by username using case-insensitive matching, or null when absent.</summary>
    ValueTask<UserEntity?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
