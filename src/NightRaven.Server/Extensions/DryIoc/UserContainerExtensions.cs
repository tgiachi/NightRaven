using DryIoc;
using NightRaven.Core.Ids;
using NightRaven.Persistence.Extensions.DryIoc;
using NightRaven.Server.Services.Users;
using NightRaven.UO.Domain.Entities;
using NightRaven.UO.Domain.Interfaces.Services;

namespace NightRaven.Server.Extensions.DryIoc;

/// <summary>
/// DryIoc-native registration helpers for UO user services.
/// </summary>
public static class UserContainerExtensions
{
    private const ushort UserEntityTypeId = 1;
    private const int UserEntitySchemaVersion = 1;

    /// <summary>
    /// Registers the UO user entity and user service available to server code and plugins.
    /// </summary>
    public static IContainer AddNightRavenUsers(this IContainer container)
    {
        container.RegisterPersistenceEntity<UserEntity, Serial>(UserEntityTypeId, UserEntitySchemaVersion, user => user.Id);
        container.Register<IUserService, UserService>(Reuse.Singleton);

        return container;
    }
}
