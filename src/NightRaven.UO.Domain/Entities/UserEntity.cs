using NightRaven.Core.Ids;
using NightRaven.UO.Domain.Types;

namespace NightRaven.UO.Domain.Entities;

public sealed class UserEntity
{
    public Serial Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public UserLevelType Level { get; set; }
    public bool IsActive { get; set; }

    public UserEntity(Serial id, string username, string password, UserLevelType level, bool isActive)
    {
        Id = id;
        Username = username;
        Password = password;
        Level = level;
        IsActive = isActive;
    }
}
