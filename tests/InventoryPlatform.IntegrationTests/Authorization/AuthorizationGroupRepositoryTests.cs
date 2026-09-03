using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Identity;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryPlatform.IntegrationTests.Authorization;

public class AuthorizationGroupRepositoryTests : IDisposable
{
    private readonly DbContextOptions<ApplicationDbContext> _options;
    private readonly ApplicationDbContext _context;
    private readonly AuthorizationGroupRepository _repository;

    public AuthorizationGroupRepositoryTests()
    {
        var dbName = $"GroupRepoTest_{Guid.NewGuid():N}";
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        _context = new ApplicationDbContext(_options);
        _repository = new AuthorizationGroupRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    // --- GetWithCapabilitiesAsync ---

    [Fact]
    public async Task GetWithCapabilitiesAsync_WhenGroupExists_ReturnsWithCapabilities()
    {
        var group = SetupGroupWithCapabilities("TestGroup", ["Cap.A", "Cap.B"]);

        var result = await _repository.GetWithCapabilitiesAsync(group.Id);

        Assert.NotNull(result);
        Assert.Equal("TestGroup", result.Name);
        Assert.Equal(2, result.Capabilities.Count);
    }

    [Fact]
    public async Task GetWithCapabilitiesAsync_WhenGroupDoesNotExist_ReturnsNull()
    {
        var result = await _repository.GetWithCapabilitiesAsync(9999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetWithCapabilitiesAsync_ReturnsCorrectCapabilityNames()
    {
        var group = SetupGroupWithCapabilities("TestGroup", ["Product.View", "Product.Create", "Category.View"]);

        var result = await _repository.GetWithCapabilitiesAsync(group.Id);

        Assert.NotNull(result);
        var capabilityNames = result.Capabilities
            .Select(rc => GetCapabilityName(rc.CapabilityId))
            .OrderBy(n => n)
            .ToList();

        Assert.Equal(3, capabilityNames.Count);
        Assert.Contains("Category.View", capabilityNames);
        Assert.Contains("Product.Create", capabilityNames);
        Assert.Contains("Product.View", capabilityNames);
    }

    [Fact]
    public async Task GetWithCapabilitiesAsync_GroupWithoutCapabilities_ReturnsEmptyCapabilities()
    {
        var group = new AuthorizationGroup("EmptyGroup");
        _context.AuthorizationGroups.Add(group);
        await _context.SaveChangesAsync();

        var result = await _repository.GetWithCapabilitiesAsync(group.Id);

        Assert.NotNull(result);
        Assert.Empty(result.Capabilities);
    }

    [Fact]
    public async Task GetWithCapabilitiesAsync_DoesNotReturnUnrelatedCapabilities()
    {
        var groupA = SetupGroupWithCapabilities("GroupA", ["Cap.A"]);
        SetupGroupWithCapabilities("GroupB", ["Cap.B"]);

        var result = await _repository.GetWithCapabilitiesAsync(groupA.Id);

        Assert.NotNull(result);
        Assert.Single(result.Capabilities);
        var capName = GetCapabilityName(result.Capabilities.First().CapabilityId);
        Assert.Equal("Cap.A", capName);
    }

    // --- GetForUserAsync ---

    [Fact]
    public async Task GetForUserAsync_UserWithOneGroup_ReturnsGroup()
    {
        var userId = Guid.NewGuid();
        var group = SetupGroupWithCapabilities("TestGroup", ["Cap.A"]);
        AssignUserToGroup(userId, group.Id);

        var result = await _repository.GetForUserAsync(userId);

        Assert.Single(result);
        Assert.Equal("TestGroup", result[0].Name);
    }

    [Fact]
    public async Task GetForUserAsync_UserWithMultipleGroups_ReturnsAllGroups()
    {
        var userId = Guid.NewGuid();
        var groupA = SetupGroupWithCapabilities("GroupA", ["Cap.A"]);
        var groupB = SetupGroupWithCapabilities("GroupB", ["Cap.B"]);
        AssignUserToGroup(userId, groupA.Id);
        AssignUserToGroup(userId, groupB.Id);

        var result = await _repository.GetForUserAsync(userId);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, g => g.Name == "GroupA");
        Assert.Contains(result, g => g.Name == "GroupB");
    }

    [Fact]
    public async Task GetForUserAsync_UserWithNoGroups_ReturnsEmpty()
    {
        var userId = Guid.NewGuid();

        var result = await _repository.GetForUserAsync(userId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetForUserAsync_ReturnGroupsWithCapabilitiesLoaded()
    {
        var userId = Guid.NewGuid();
        var group = SetupGroupWithCapabilities("TestGroup", ["Cap.A", "Cap.B"]);
        AssignUserToGroup(userId, group.Id);

        var result = await _repository.GetForUserAsync(userId);

        Assert.Single(result);
        Assert.Equal(2, result[0].Capabilities.Count);
    }

    [Fact]
    public async Task GetForUserAsync_DoesNotReturnUnassignedGroups()
    {
        var userId = Guid.NewGuid();
        var groupA = SetupGroupWithCapabilities("GroupA", ["Cap.A"]);
        SetupGroupWithCapabilities("GroupB", ["Cap.B"]);
        AssignUserToGroup(userId, groupA.Id);

        var result = await _repository.GetForUserAsync(userId);

        Assert.Single(result);
        Assert.Equal("GroupA", result[0].Name);
    }

    [Fact]
    public async Task GetForUserAsync_UserAssignedToSameGroupTwice_ReturnsSingleGroup()
    {
        var userId = Guid.NewGuid();
        var group = SetupGroupWithCapabilities("TestGroup", ["Cap.A"]);
        AssignUserToGroup(userId, group.Id);
        AssignUserToGroup(userId, group.Id); // duplicate assignment

        var result = await _repository.GetForUserAsync(userId);

        Assert.Single(result);
    }

    // --- GetWithCapabilitiesAndUsersAsync ---

    [Fact]
    public async Task GetWithCapabilitiesAndUsersAsync_WhenGroupExists_ReturnsWithCapabilitiesAndUsers()
    {
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var group = SetupGroupWithCapabilities("TestGroup", ["Cap.A", "Cap.B"]);
        AssignUserToGroup(user1, group.Id);
        AssignUserToGroup(user2, group.Id);

        var result = await _repository.GetWithCapabilitiesAndUsersAsync(group.Id);

        Assert.NotNull(result);
        Assert.Equal("TestGroup", result.Name);
        Assert.Equal(2, result.Capabilities.Count);
        Assert.Equal(2, result.UserGroups.Count);
    }

    [Fact]
    public async Task GetWithCapabilitiesAndUsersAsync_WhenGroupDoesNotExist_ReturnsNull()
    {
        var result = await _repository.GetWithCapabilitiesAndUsersAsync(9999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetWithCapabilitiesAndUsersAsync_ReturnsCorrectUserIds()
    {
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var group = SetupGroupWithCapabilities("TestGroup", ["Cap.A"]);
        AssignUserToGroup(user1, group.Id);
        AssignUserToGroup(user2, group.Id);

        var result = await _repository.GetWithCapabilitiesAndUsersAsync(group.Id);

        Assert.NotNull(result);
        var userIds = result.UserGroups.Select(ug => ug.UserId).ToList();
        Assert.Contains(user1, userIds);
        Assert.Contains(user2, userIds);
    }

    [Fact]
    public async Task GetWithCapabilitiesAndUsersAsync_GroupWithoutUsers_ReturnsEmptyUsers()
    {
        var group = SetupGroupWithCapabilities("TestGroup", ["Cap.A"]);

        var result = await _repository.GetWithCapabilitiesAndUsersAsync(group.Id);

        Assert.NotNull(result);
        Assert.Empty(result.UserGroups);
        Assert.Single(result.Capabilities);
    }

    [Fact]
    public async Task GetWithCapabilitiesAndUsersAsync_GroupWithoutCapabilities_ReturnsEmptyCapabilities()
    {
        var user = Guid.NewGuid();
        var group = new AuthorizationGroup("EmptyCapGroup");
        _context.AuthorizationGroups.Add(group);
        await _context.SaveChangesAsync();
        AssignUserToGroup(user, group.Id);

        var result = await _repository.GetWithCapabilitiesAndUsersAsync(group.Id);

        Assert.NotNull(result);
        Assert.Empty(result.Capabilities);
        Assert.Single(result.UserGroups);
    }

    // --- GetAllWithDetailsAsync ---

    [Fact]
    public async Task GetAllWithDetailsAsync_ReturnsAllGroups()
    {
        SetupGroupWithCapabilities("GroupA", ["Cap.A"]);
        SetupGroupWithCapabilities("GroupB", ["Cap.B"]);
        SetupGroupWithCapabilities("GroupC", ["Cap.C"]);

        var result = await _repository.GetAllWithDetailsAsync();

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAllWithDetailsAsync_ReturnsGroupsWithCapabilitiesLoaded()
    {
        SetupGroupWithCapabilities("GroupA", ["Cap.A", "Cap.B"]);
        SetupGroupWithCapabilities("GroupB", ["Cap.C"]);

        var result = await _repository.GetAllWithDetailsAsync();

        var groupA = result.Single(g => g.Name == "GroupA");
        var groupB = result.Single(g => g.Name == "GroupB");

        Assert.Equal(2, groupA.Capabilities.Count);
        Assert.Single(groupB.Capabilities);
    }

    [Fact]
    public async Task GetAllWithDetailsAsync_ReturnsGroupsWithUsersLoaded()
    {
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var groupA = SetupGroupWithCapabilities("GroupA", ["Cap.A"]);
        var groupB = SetupGroupWithCapabilities("GroupB", ["Cap.B"]);
        AssignUserToGroup(user1, groupA.Id);
        AssignUserToGroup(user2, groupB.Id);

        var result = await _repository.GetAllWithDetailsAsync();

        var groupAResult = result.Single(g => g.Name == "GroupA");
        var groupBResult = result.Single(g => g.Name == "GroupB");

        Assert.Single(groupAResult.UserGroups);
        Assert.Equal(user1, groupAResult.UserGroups.First().UserId);
        Assert.Single(groupBResult.UserGroups);
        Assert.Equal(user2, groupBResult.UserGroups.First().UserId);
    }

    [Fact]
    public async Task GetAllWithDetailsAsync_RelationshipsDoNotBleedBetweenGroups()
    {
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var groupA = SetupGroupWithCapabilities("GroupA", ["Cap.A"]);
        var groupB = SetupGroupWithCapabilities("GroupB", ["Cap.B"]);
        AssignUserToGroup(user1, groupA.Id);
        AssignUserToGroup(user2, groupB.Id);

        var result = await _repository.GetAllWithDetailsAsync();

        var groupAResult = result.Single(g => g.Name == "GroupA");
        var groupBResult = result.Single(g => g.Name == "GroupB");

        // GroupA should only have Cap.A, not Cap.B
        Assert.Single(groupAResult.Capabilities);
        var groupACapName = GetCapabilityName(groupAResult.Capabilities.First().CapabilityId);
        Assert.Equal("Cap.A", groupACapName);

        // GroupB should only have Cap.B, not Cap.A
        Assert.Single(groupBResult.Capabilities);
        var groupBCapName = GetCapabilityName(groupBResult.Capabilities.First().CapabilityId);
        Assert.Equal("Cap.B", groupBCapName);

        // Users should not bleed
        Assert.Single(groupAResult.UserGroups);
        Assert.Equal(user1, groupAResult.UserGroups.First().UserId);
        Assert.Single(groupBResult.UserGroups);
        Assert.Equal(user2, groupBResult.UserGroups.First().UserId);
    }

    [Fact]
    public async Task GetAllWithDetailsAsync_EmptyDatabase_ReturnsEmpty()
    {
        var result = await _repository.GetAllWithDetailsAsync();

        Assert.Empty(result);
    }

    // --- Base Repository: GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_WhenGroupExists_ReturnsGroup()
    {
        var group = new AuthorizationGroup("TestGroup");
        _context.AuthorizationGroups.Add(group);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(group.Id);

        Assert.NotNull(result);
        Assert.Equal("TestGroup", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenGroupDoesNotExist_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(9999);

        Assert.Null(result);
    }

    // --- Helpers ---

    private AuthorizationGroup SetupGroupWithCapabilities(string groupName, string[] capabilityNames)
    {
        var group = new AuthorizationGroup(groupName);
        _context.AuthorizationGroups.Add(group);
        _context.SaveChanges();

        foreach (var name in capabilityNames)
        {
            var capability = new Capability(name);
            _context.Capabilities.Add(capability);
            _context.SaveChanges();

            group.AddCapability(capability);
        }

        _context.SaveChanges();
        return group;
    }

    private void AssignUserToGroup(Guid userId, int groupId)
    {
        var group = _context.AuthorizationGroups
            .Include(g => g.UserGroups)
            .Single(g => g.Id == groupId);

        group.AssignUser(userId);
        _context.SaveChanges();
    }

    private string GetCapabilityName(int capabilityId)
    {
        return _context.Capabilities
            .AsNoTracking()
            .Where(c => c.Id == capabilityId)
            .Select(c => c.Name)
            .Single();
    }
}
