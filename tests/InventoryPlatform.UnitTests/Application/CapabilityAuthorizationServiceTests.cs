using InventoryPlatform.Application.Authorization;
using InventoryPlatform.Application.Interfaces.Authorization;
using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Common;
using InventoryPlatform.Domain.Entities;
using System.Linq.Expressions;
using Xunit;

namespace InventoryPlatform.UnitTests.Application;

public class CapabilityAuthorizationServiceTests
{
    private const string ViewCapability = "Product.View";
    private const string CreateCapability = "Product.Create";

    private static readonly Guid User1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid User2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

    // --- Capability Granted ---

    [Fact]
    public async Task HasCapabilityAsync_WhenUserHasEnabledCapability_ReturnsTrue()
    {
        var capability = CreatePersistedCapability(ViewCapability, isEnabled: true);
        var group = CreateGroupWithCapability(1, capability);

        var service = CreateService(
            capabilityLookup: capability,
            userGroups: [group]);

        var result = await service.HasCapabilityAsync(User1, ViewCapability);

        Assert.True(result);
    }

    // --- Capability Not Granted ---

    [Fact]
    public async Task HasCapabilityAsync_WhenUserHasNoMatchingCapability_ReturnsFalse()
    {
        // Capability exists and is enabled, but is not in any of user's groups
        var categoryViewCap = CreatePersistedCapability("Category.View", isEnabled: true);
        var viewCap = CreatePersistedCapability(ViewCapability, isEnabled: true);
        var group = CreateGroupWithCapability(1, viewCap);

        var service = CreateService(
            capabilityLookup: categoryViewCap,
            userGroups: [group]);

        // Request categoryViewCap which exists but user's group only has viewCap
        var result = await service.HasCapabilityAsync(User1, "Category.View");

        Assert.False(result);
    }

    // --- Disabled Capability ---

    [Fact]
    public async Task HasCapabilityAsync_WhenCapabilityIsDisabled_ReturnsFalse()
    {
        var capability = CreatePersistedCapability(ViewCapability, isEnabled: false);
        var group = CreateGroupWithCapability(1, capability);

        var service = CreateService(
            capabilityLookup: capability,
            userGroups: [group]);

        var result = await service.HasCapabilityAsync(User1, ViewCapability);

        Assert.False(result);
    }

    // --- Multiple Groups ---

    [Fact]
    public async Task HasCapabilityAsync_WhenCapabilityInSecondGroup_ReturnsTrue()
    {
        var viewCap = CreatePersistedCapability(ViewCapability, isEnabled: true);
        var createCap = CreatePersistedCapability(CreateCapability, isEnabled: true);

        var groupA = CreateGroupWithCapability(1, viewCap);
        var groupB = CreateGroupWithCapability(2, createCap);

        var service = CreateService(
            capabilityLookup: createCap,
            userGroups: [groupA, groupB]);

        // User is in two groups; capability is only in groupB
        var result = await service.HasCapabilityAsync(User1, CreateCapability);

        Assert.True(result);
    }

    [Fact]
    public async Task HasCapabilityAsync_WhenCapabilityAbsentFromAllGroups_ReturnsFalse()
    {
        var viewCap = CreatePersistedCapability(ViewCapability, isEnabled: true);
        var createCap = CreatePersistedCapability(CreateCapability, isEnabled: true);

        var groupA = CreateGroupWithCapability(1, viewCap);
        var groupB = CreateGroupWithCapability(2, viewCap);

        var service = CreateService(
            capabilityLookup: createCap,
            userGroups: [groupA, groupB]);

        // createCap exists but is not in any of user's groups
        var result = await service.HasCapabilityAsync(User1, CreateCapability);

        Assert.False(result);
    }

    // --- Multiple Capabilities Within a Group ---

    [Fact]
    public async Task HasCapabilityAsync_WhenGroupHasMultipleCapabilities_ReturnsTrueForAny()
    {
        var viewCap = CreatePersistedCapability(ViewCapability, isEnabled: true);
        var createCap = CreatePersistedCapability(CreateCapability, isEnabled: true);
        var group = CreateGroupWithCapabilities(1, [viewCap, createCap]);

        var service = CreateService(
            capabilityLookup: viewCap,
            userGroups: [group]);

        var result = await service.HasCapabilityAsync(User1, ViewCapability);

        Assert.True(result);
    }

    // --- User With No Authorization Groups ---

    [Fact]
    public async Task HasCapabilityAsync_WhenUserHasNoGroups_ReturnsFalse()
    {
        var capability = CreatePersistedCapability(ViewCapability, isEnabled: true);

        var service = CreateService(
            capabilityLookup: capability,
            userGroups: []);

        var result = await service.HasCapabilityAsync(User1, ViewCapability);

        Assert.False(result);
    }

    // --- Empty User ID ---

    [Fact]
    public async Task HasCapabilityAsync_WhenUserIdIsEmpty_ReturnsFalse()
    {
        var service = CreateService(
            capabilityLookup: CreatePersistedCapability(ViewCapability),
            userGroups: []);

        var result = await service.HasCapabilityAsync(Guid.Empty, ViewCapability);

        Assert.False(result);
    }

    // --- Unknown Capability ---

    [Fact]
    public async Task HasCapabilityAsync_WhenCapabilityNotFound_ReturnsFalse()
    {
        var service = CreateService(
            capabilityLookup: null,
            userGroups: []);

        var result = await service.HasCapabilityAsync(User1, "Nonexistent.Capability");

        Assert.False(result);
    }

    // --- Null / Whitespace Capability Name ---

    [Fact]
    public async Task HasCapabilityAsync_WhenCapabilityNameIsNull_ReturnsFalse()
    {
        var service = CreateService(
            capabilityLookup: null,
            userGroups: []);

        var result = await service.HasCapabilityAsync(User1, null!);

        Assert.False(result);
    }

    [Fact]
    public async Task HasCapabilityAsync_WhenCapabilityNameIsEmpty_ReturnsFalse()
    {
        var service = CreateService(
            capabilityLookup: null,
            userGroups: []);

        var result = await service.HasCapabilityAsync(User1, string.Empty);

        Assert.False(result);
    }

    [Fact]
    public async Task HasCapabilityAsync_WhenCapabilityNameIsWhitespace_ReturnsFalse()
    {
        var service = CreateService(
            capabilityLookup: null,
            userGroups: []);

        var result = await service.HasCapabilityAsync(User1, "   ");

        Assert.False(result);
    }

    // --- Repository Interaction ---

    [Fact]
    public async Task HasCapabilityAsync_CallsGetByNameWithCorrectName()
    {
        var fake = new FakeCapabilityRepository();
        var groupRepo = new FakeAuthorizationGroupRepository();
        var service = new CapabilityAuthorizationService(fake, groupRepo);

        await service.HasCapabilityAsync(User1, ViewCapability);

        Assert.Equal(ViewCapability, fake.LastQueriedName);
    }

    [Fact]
    public async Task HasCapabilityAsync_CallsGetForUserWithCorrectUserId()
    {
        var capability = CreatePersistedCapability(ViewCapability);
        var fake = new FakeCapabilityRepository(capability);
        var groupRepo = new FakeAuthorizationGroupRepository();
        var service = new CapabilityAuthorizationService(fake, groupRepo);

        await service.HasCapabilityAsync(User2, ViewCapability);

        Assert.Equal(User2, groupRepo.LastQueriedUserId);
    }

    [Fact]
    public async Task HasCapabilityAsync_WhenCapabilityNotFound_DoesNotCallGroupRepository()
    {
        var fake = new FakeCapabilityRepository(null);
        var groupRepo = new FakeAuthorizationGroupRepository();
        var service = new CapabilityAuthorizationService(fake, groupRepo);

        await service.HasCapabilityAsync(User1, "Nonexistent.Cap");

        Assert.Null(groupRepo.LastQueriedUserId);
    }

    // --- Helpers ---

    private static CapabilityAuthorizationService CreateService(
        Capability? capabilityLookup,
        IReadOnlyList<AuthorizationGroup> userGroups)
    {
        var capabilityRepo = new FakeCapabilityRepository(capabilityLookup);
        var groupRepo = new FakeAuthorizationGroupRepository(userGroups);
        return new CapabilityAuthorizationService(capabilityRepo, groupRepo);
    }

    private static int _nextId = 1;

    private static Capability CreatePersistedCapability(string name, bool isEnabled = true)
    {
        var capability = new Capability(name);
        SetEntityId(capability, _nextId++);
        if (!isEnabled)
        {
            capability.Disable();
        }
        return capability;
    }

    private static AuthorizationGroup CreateGroupWithCapability(int groupId, Capability capability)
    {
        var group = new AuthorizationGroup($"Group_{groupId}");
        SetEntityId(group, groupId);
        group.AddCapability(capability);
        return group;
    }

    private static AuthorizationGroup CreateGroupWithCapabilities(int groupId, IReadOnlyList<Capability> capabilities)
    {
        var group = new AuthorizationGroup($"Group_{groupId}");
        SetEntityId(group, groupId);
        foreach (var cap in capabilities)
        {
            group.AddCapability(cap);
        }
        return group;
    }

    private static void SetEntityId(BaseEntity entity, int id)
    {
        typeof(BaseEntity)
            .GetProperty(nameof(BaseEntity.Id))!
            .SetValue(entity, id);
    }

    // --- Fake Implementations ---

    private class FakeCapabilityRepository : ICapabilityRepository
    {
        private readonly Capability? _capability;

        public string? LastQueriedName { get; private set; }

        public FakeCapabilityRepository(Capability? capability = null) => _capability = capability;

        public Task<Capability?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            LastQueriedName = name;
            return Task.FromResult(_capability);
        }

        public Task<Capability?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult<Capability?>(null);

        public Task<IReadOnlyList<Capability>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Capability>>([]);

        public Task<IReadOnlyList<Capability>> FindAsync(Expression<Func<Capability, bool>> predicate, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Capability>>([]);

        public Task AddAsync(Capability entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Update(Capability entity) { }

        public void Remove(Capability entity) { }

        public Task<bool> ExistsAsync(Expression<Func<Capability, bool>> predicate, CancellationToken cancellationToken = default)
            => Task.FromResult(false);
    }

    private class FakeAuthorizationGroupRepository : IAuthorizationGroupRepository
    {
        private readonly IReadOnlyList<AuthorizationGroup> _groups;

        public Guid? LastQueriedUserId { get; private set; }

        public FakeAuthorizationGroupRepository(IReadOnlyList<AuthorizationGroup>? groups = null)
        {
            _groups = groups ?? [];
        }

        public Task<IReadOnlyList<AuthorizationGroup>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            LastQueriedUserId = userId;
            return Task.FromResult<IReadOnlyList<AuthorizationGroup>>(_groups);
        }

        public Task<AuthorizationGroup?> GetWithCapabilitiesAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult<AuthorizationGroup?>(null);

        public Task<AuthorizationGroup?> GetWithCapabilitiesAndUsersAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult<AuthorizationGroup?>(null);

        public Task<IReadOnlyList<AuthorizationGroup>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AuthorizationGroup>>([]);

        public Task<AuthorizationGroup?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
            => Task.FromResult<AuthorizationGroup?>(null);

        public Task<IReadOnlyList<AuthorizationGroup>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AuthorizationGroup>>([]);

        public Task<IReadOnlyList<AuthorizationGroup>> FindAsync(Expression<Func<AuthorizationGroup, bool>> predicate, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AuthorizationGroup>>([]);

        public Task AddAsync(AuthorizationGroup entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Update(AuthorizationGroup entity) { }

        public void Remove(AuthorizationGroup entity) { }

        public Task<bool> ExistsAsync(Expression<Func<AuthorizationGroup, bool>> predicate, CancellationToken cancellationToken = default)
            => Task.FromResult(false);
    }
}
