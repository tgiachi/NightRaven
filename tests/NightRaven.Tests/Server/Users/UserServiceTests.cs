using DryIoc;
using NightRaven.Abstractions.Interfaces.Events;
using NightRaven.Abstractions.Interfaces.Services;
using NightRaven.Core.Ids;
using NightRaven.Core.Utils;
using NightRaven.Persistence.Interfaces.Persistence;
using NightRaven.Server.Extensions.Configuration;
using NightRaven.Server.Extensions.EventBus;
using NightRaven.Server.Extensions.Persistence;
using NightRaven.Server.Extensions.Users;
using NightRaven.Server.Services.Users;
using NightRaven.Tests.Support;
using NightRaven.UO.Domain.Entities;
using NightRaven.UO.Domain.Events;
using NightRaven.UO.Domain.Interfaces.Services;
using NightRaven.UO.Domain.Types;

namespace NightRaven.Tests.Server.Users;

public sealed class UserServiceTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), $"nr-users-{Guid.NewGuid():N}");
    private string ConfigPath => Path.Combine(_dir, "nightraven.toml");

    private sealed class CapturingEventBusService : IEventBusService
    {
        public List<IAsyncEvent> AsyncEvents { get; } = [];
        public Action<Type, Exception, INightRavenEvent>? OnEventError { get; set; }
        public int CurrentTickQueueDepth => 0;

        public int DrainTickEvents(int maxItems)
            => 0;

        public void Publish<TEvent>(TEvent evt)
            where TEvent : ITickEvent { }

        public Task PublishAsync<TEvent>(TEvent evt, CancellationToken cancellationToken = default)
            where TEvent : IAsyncEvent
        {
            AsyncEvents.Add(evt);

            return Task.CompletedTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class FakeUserAccess : IAutoDataAccess<UserEntity, Serial>
    {
        private readonly Dictionary<Serial, UserEntity> _users = [];
        private uint _nextId = 1;

        public IReadOnlyCollection<UserEntity> Users => _users.Values.ToArray();

        public void Add(UserEntity user)
        {
            _users[user.Id] = Clone(user);

            if (user.Id.Value >= _nextId)
            {
                _nextId = user.Id.Value + 1;
            }
        }

        public ValueTask<int> CountAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_users.Count);

        public ValueTask<IReadOnlyCollection<UserEntity>> GetAllAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult<IReadOnlyCollection<UserEntity>>(_users.Values.Select(Clone).ToArray());

        public ValueTask<UserEntity?> GetByIdAsync(Serial id, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_users.TryGetValue(id, out var user) ? Clone(user) : null);

        public ValueTask<Serial> NextIdAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult(new Serial(_nextId++));

        public IQueryable<UserEntity> Query()
            => _users.Values.Select(Clone).AsQueryable();

        public ValueTask<bool> RemoveAsync(Serial id, CancellationToken cancellationToken = default)
            => ValueTask.FromResult(_users.Remove(id));

        public ValueTask UpsertAsync(UserEntity entity, CancellationToken cancellationToken = default)
        {
            _users[entity.Id] = Clone(entity);

            return ValueTask.CompletedTask;
        }

        private static UserEntity Clone(UserEntity user)
            => new(user.Id, user.Username, user.Password, user.Level, user.IsActive);
    }

    [Fact]
    public async Task AddNightRavenUsers_RegistersServiceAndUserPersistence()
    {
        Directory.CreateDirectory(_dir);
        File.WriteAllText(ConfigPath, "[persistence]\nenable_file_lock = false\n");

        var container = new Container();
        container.AddNightRavenEventBus();
        container.AddNightRavenUsers();
        container.AddNightRavenPersistence(_dir);
        container.AddNightRavenConfig(ConfigPath);

        var orchestrator = container.Orchestrator();
        await orchestrator.StartAsync(CancellationToken.None);

        try
        {
            var service = container.Resolve<IUserService>();

            var user = await service.CreateAsync("ContainerUser", "secret");

            Assert.Equal(new(1), user.Id);
            Assert.True(HashUtils.VerifyPassword("secret", user.Password));
        }
        finally
        {
            await orchestrator.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task CountAsync_ReturnsPersistedUserCount()
    {
        var access = new FakeUserAccess();
        access.Add(new(new(1), "one", HashUtils.HashPassword("secret"), UserLevelType.Player, true));
        access.Add(new(new(2), "two", HashUtils.HashPassword("secret"), UserLevelType.Player, true));
        var service = new UserService(access, new CapturingEventBusService());

        var count = await service.CountAsync();

        Assert.Equal(2, count);
    }

    [Fact]
    public async Task CreateAsync_DuplicateUsernameCaseInsensitive_ThrowsAndDoesNotPublish()
    {
        var access = new FakeUserAccess();
        access.Add(new(new(7), "Arthorius", HashUtils.HashPassword("old"), UserLevelType.Player, true));
        var bus = new CapturingEventBusService();
        var service = new UserService(access, bus);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.CreateAsync("arthorius", "secret"));

        Assert.Single(access.Users);
        Assert.Empty(bus.AsyncEvents);
    }

    [Fact]
    public async Task CreateAsync_HashesPasswordPersistsUserAndPublishesCreatedEvent()
    {
        var access = new FakeUserAccess();
        var bus = new CapturingEventBusService();
        var service = new UserService(access, bus);

        var user = await service.CreateAsync("Arthorius", "secret", UserLevelType.GameMaster);

        Assert.Equal(new(1), user.Id);
        Assert.Equal("Arthorius", user.Username);
        Assert.Equal(UserLevelType.GameMaster, user.Level);
        Assert.True(user.IsActive);
        Assert.NotEqual("secret", user.Password);
        Assert.True(HashUtils.VerifyPassword("secret", user.Password));

        var stored = Assert.Single(access.Users);
        Assert.Equal(user.Id, stored.Id);
        Assert.Equal(user.Password, stored.Password);

        var created = Assert.Single(bus.AsyncEvents.OfType<UserCreatedEvent>());
        Assert.Equal(user.Id, created.UserId);
        Assert.Equal(user.Username, created.Username);
        Assert.Equal(user.Level, created.Level);
        Assert.True(created.IsActive);
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir))
        {
            Directory.Delete(_dir, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetByUsernameAsync_MatchesUsernameCaseInsensitively()
    {
        var access = new FakeUserAccess();
        access.Add(new(new(42), "Arthorius", HashUtils.HashPassword("secret"), UserLevelType.Player, true));
        var service = new UserService(access, new CapturingEventBusService());

        var user = await service.GetByUsernameAsync("arthorius");

        Assert.NotNull(user);
        Assert.Equal(new(42), user!.Id);
    }
}
