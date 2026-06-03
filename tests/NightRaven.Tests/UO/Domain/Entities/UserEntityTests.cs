using NightRaven.Core.Ids;
using NightRaven.UO.Domain.Entities;
using NightRaven.UO.Domain.Types;

namespace NightRaven.Tests.UO.Domain.Entities;

public class UserEntityTests
{
    [Fact]
    public void Constructor_ValidArgs_SetsAllProperties()
    {
        var id = new Serial(1);

        var user = new UserEntity(id, "arthorius", "hashed_pw", UserLevelType.Player, isActive: true);

        Assert.Equal(id, user.Id);
        Assert.Equal("arthorius", user.Username);
        Assert.Equal("hashed_pw", user.Password);
        Assert.Equal(UserLevelType.Player, user.Level);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Constructor_InactiveUser_IsActiveIsFalse()
    {
        var user = new UserEntity(new Serial(2), "banned", "pw", UserLevelType.Player, isActive: false);

        Assert.False(user.IsActive);
    }

    [Fact]
    public void Constructor_AdminLevel_LevelIsAdministrator()
    {
        var user = new UserEntity(new Serial(3), "admin", "pw", UserLevelType.Administrator, isActive: true);

        Assert.Equal(UserLevelType.Administrator, user.Level);
    }

    [Fact]
    public void IsActive_SetToFalse_ReflectsChange()
    {
        var user = new UserEntity(new Serial(4), "user", "pw", UserLevelType.Player, isActive: true);

        user.IsActive = false;

        Assert.False(user.IsActive);
    }

    [Fact]
    public void Level_Promoted_ReflectsChange()
    {
        var user = new UserEntity(new Serial(5), "user", "pw", UserLevelType.Player, isActive: true);

        user.Level = UserLevelType.GameMaster;

        Assert.Equal(UserLevelType.GameMaster, user.Level);
    }

    [Fact]
    public void Password_Updated_ReflectsChange()
    {
        var user = new UserEntity(new Serial(6), "user", "old_hash", UserLevelType.Player, isActive: true);

        user.Password = "new_hash";

        Assert.Equal("new_hash", user.Password);
    }

    [Fact]
    public void Id_DifferentSerials_AreNotEqual()
    {
        var a = new UserEntity(new Serial(10), "a", "pw", UserLevelType.Player, isActive: true);
        var b = new UserEntity(new Serial(11), "b", "pw", UserLevelType.Player, isActive: true);

        Assert.NotEqual(a.Id, b.Id);
    }
}
