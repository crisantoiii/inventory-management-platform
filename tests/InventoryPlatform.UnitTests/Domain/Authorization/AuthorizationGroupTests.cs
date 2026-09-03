using InventoryPlatform.Domain.Common;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Exceptions;
using Xunit;

namespace InventoryPlatform.UnitTests.Domain.Authorization;

public class AuthorizationGroupTests
{
    [Fact]
    public void Constructor_WithValidName_CreatesGroup()
    {
        var group = new AuthorizationGroup("Administrator");

        Assert.True(group.Id >= 0);
        Assert.Equal("Administrator", group.Name);
        Assert.Empty(group.Capabilities);
        Assert.Empty(group.UserGroups);
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new AuthorizationGroup(string.Empty));
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new AuthorizationGroup(null!));
    }

    [Fact]
    public void Constructor_WithWhitespaceName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new AuthorizationGroup("   "));
    }

    [Fact]
    public void Rename_WithValidName_UpdatesName()
    {
        var group = new AuthorizationGroup("OldName");

        group.Rename("NewName");

        Assert.Equal("NewName", group.Name);
    }

    [Fact]
    public void Rename_WithEmptyName_ThrowsArgumentException()
    {
        var group = new AuthorizationGroup("ValidName");

        Assert.Throws<ArgumentException>(() => group.Rename(string.Empty));
    }

    [Fact]
    public void Rename_WithNullName_ThrowsArgumentException()
    {
        var group = new AuthorizationGroup("ValidName");

        Assert.Throws<ArgumentException>(() => group.Rename(null!));
    }

    // --- AddCapability ---

    [Fact]
    public void AddCapability_WithValidCapability_AddsToGroup()
    {
        var group = CreatePersistedGroup("TestGroup");
        var capability = CreatePersistedCapability("Product.View");

        group.AddCapability(capability);

        Assert.Single(group.Capabilities);
        Assert.Equal(capability.Id, group.Capabilities.First().CapabilityId);
    }

    [Fact]
    public void AddCapability_WithNullCapability_ThrowsArgumentNullException()
    {
        var group = CreatePersistedGroup("TestGroup");

        Assert.Throws<ArgumentNullException>(() => group.AddCapability(null!));
    }

    [Fact]
    public void AddCapability_WithUnpersistedCapability_ThrowsDomainException()
    {
        var group = CreatePersistedGroup("TestGroup");
        var unpersistedCapability = new Capability("Test.Cap");

        var ex = Assert.Throws<DomainException>(() => group.AddCapability(unpersistedCapability));
        Assert.Contains("persisted", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddCapability_DuplicateCapability_IsIdempotent()
    {
        var group = CreatePersistedGroup("TestGroup");
        var capability = CreatePersistedCapability("Product.View");

        group.AddCapability(capability);
        group.AddCapability(capability);

        Assert.Single(group.Capabilities);
    }

    [Fact]
    public void AddCapability_MultipleDifferentCapabilities_AllAdded()
    {
        var group = CreatePersistedGroup("TestGroup");
        var cap1 = CreatePersistedCapability("Product.View");
        var cap2 = CreatePersistedCapability("Product.Create");
        var cap3 = CreatePersistedCapability("Product.Edit");

        group.AddCapability(cap1);
        group.AddCapability(cap2);
        group.AddCapability(cap3);

        Assert.Equal(3, group.Capabilities.Count);
    }

    [Fact]
    public void AddCapability_CapabilityRelationshipReflectsGroupId()
    {
        var group = CreatePersistedGroup("TestGroup");
        var capability = CreatePersistedCapability("Product.View");

        group.AddCapability(capability);

        var relationship = group.Capabilities.First();
        Assert.Equal(group.Id, relationship.AuthorizationGroupId);
        Assert.Equal(capability.Id, relationship.CapabilityId);
    }

    [Fact]
    public void AddCapability_CapabilityGroupCapabilitiesCollectionIsUpdated()
    {
        var group = CreatePersistedGroup("TestGroup");
        var capability = CreatePersistedCapability("Product.View");

        group.AddCapability(capability);

        Assert.Single(capability.GroupCapabilities);
    }

    // --- RemoveCapability ---

    [Fact]
    public void RemoveCapability_ExistingCapability_RemovesFromGroup()
    {
        var group = CreatePersistedGroup("TestGroup");
        var capability = CreatePersistedCapability("Product.View");
        group.AddCapability(capability);

        group.RemoveCapability(capability.Id);

        Assert.Empty(group.Capabilities);
    }

    [Fact]
    public void RemoveCapability_NonexistentCapabilityId_DoesNothing()
    {
        var group = CreatePersistedGroup("TestGroup");
        var capability = CreatePersistedCapability("Product.View");
        group.AddCapability(capability);

        group.RemoveCapability(9999);

        Assert.Single(group.Capabilities);
    }

    [Fact]
    public void RemoveCapability_WithZeroId_ThrowsArgumentOutOfRangeException()
    {
        var group = CreatePersistedGroup("TestGroup");

        Assert.Throws<ArgumentOutOfRangeException>(() => group.RemoveCapability(0));
    }

    [Fact]
    public void RemoveCapability_WithNegativeId_ThrowsArgumentOutOfRangeException()
    {
        var group = CreatePersistedGroup("TestGroup");

        Assert.Throws<ArgumentOutOfRangeException>(() => group.RemoveCapability(-1));
    }

    [Fact]
    public void RemoveCapability_OnlyRemovesSpecifiedCapability()
    {
        var group = CreatePersistedGroup("TestGroup");
        var cap1 = CreatePersistedCapability("Product.View");
        var cap2 = CreatePersistedCapability("Product.Create");
        group.AddCapability(cap1);
        group.AddCapability(cap2);

        group.RemoveCapability(cap1.Id);

        Assert.Single(group.Capabilities);
        Assert.Equal(cap2.Id, group.Capabilities.First().CapabilityId);
    }

    // --- AssignUser ---

    [Fact]
    public void AssignUser_WithValidUserId_AddsUserToGroup()
    {
        var group = CreatePersistedGroup("TestGroup");
        var userId = Guid.NewGuid();

        group.AssignUser(userId);

        Assert.Single(group.UserGroups);
        Assert.Equal(userId, group.UserGroups.First().UserId);
    }

    [Fact]
    public void AssignUser_WithEmptyGuid_ThrowsArgumentException()
    {
        var group = CreatePersistedGroup("TestGroup");

        Assert.Throws<ArgumentException>(() => group.AssignUser(Guid.Empty));
    }

    [Fact]
    public void AssignUser_DuplicateUser_IsIdempotent()
    {
        var group = CreatePersistedGroup("TestGroup");
        var userId = Guid.NewGuid();

        group.AssignUser(userId);
        group.AssignUser(userId);

        Assert.Single(group.UserGroups);
    }

    [Fact]
    public void AssignUser_MultipleDifferentUsers_AllAdded()
    {
        var group = CreatePersistedGroup("TestGroup");
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var user3 = Guid.NewGuid();

        group.AssignUser(user1);
        group.AssignUser(user2);
        group.AssignUser(user3);

        Assert.Equal(3, group.UserGroups.Count);
    }

    [Fact]
    public void AssignUser_UserGroupRelationshipReflectsIds()
    {
        var group = CreatePersistedGroup("TestGroup");
        var userId = Guid.NewGuid();

        group.AssignUser(userId);

        var relationship = group.UserGroups.First();
        Assert.Equal(userId, relationship.UserId);
        Assert.Equal(group.Id, relationship.AuthorizationGroupId);
    }

    // --- RemoveUser ---

    [Fact]
    public void RemoveUser_ExistingUser_RemovesFromGroup()
    {
        var group = CreatePersistedGroup("TestGroup");
        var userId = Guid.NewGuid();
        group.AssignUser(userId);

        group.RemoveUser(userId);

        Assert.Empty(group.UserGroups);
    }

    [Fact]
    public void RemoveUser_NonexistentUser_DoesNothing()
    {
        var group = CreatePersistedGroup("TestGroup");
        var userId = Guid.NewGuid();
        group.AssignUser(userId);

        group.RemoveUser(Guid.NewGuid());

        Assert.Single(group.UserGroups);
    }

    [Fact]
    public void RemoveUser_WithEmptyGuid_ThrowsArgumentException()
    {
        var group = CreatePersistedGroup("TestGroup");

        Assert.Throws<ArgumentException>(() => group.RemoveUser(Guid.Empty));
    }

    [Fact]
    public void RemoveUser_OnlyRemovesSpecifiedUser()
    {
        var group = CreatePersistedGroup("TestGroup");
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        group.AssignUser(user1);
        group.AssignUser(user2);

        group.RemoveUser(user1);

        Assert.Single(group.UserGroups);
        Assert.Equal(user2, group.UserGroups.First().UserId);
    }

    // --- Collections ---

    [Fact]
    public void Capabilities_IsReadOnly()
    {
        var group = new AuthorizationGroup("TestGroup");

        Assert.IsAssignableFrom<IReadOnlyCollection<AuthorizationGroupCapability>>(group.Capabilities);
    }

    [Fact]
    public void UserGroups_IsReadOnly()
    {
        var group = new AuthorizationGroup("TestGroup");

        Assert.IsAssignableFrom<IReadOnlyCollection<UserAuthorizationGroup>>(group.UserGroups);
    }

    /// <summary>
    /// Creates an AuthorizationGroup with a simulated persisted ID for testing.
    /// Uses reflection to set the Id since BaseEntity.Id has a protected setter.
    /// </summary>
    private static AuthorizationGroup CreatePersistedGroup(string name)
    {
        var group = new AuthorizationGroup(name);
        SetEntityId(group, 1);
        return group;
    }

    private static int _nextId = 1;

    /// <summary>
    /// Creates a Capability with a simulated persisted ID for testing.
    /// Uses reflection to set the Id since BaseEntity.Id has a protected setter.
    /// </summary>
    private static Capability CreatePersistedCapability(string name)
    {
        var capability = new Capability(name);
        SetEntityId(capability, _nextId++);
        return capability;
    }

    private static void SetEntityId(BaseEntity entity, int id)
    {
        typeof(BaseEntity)
            .GetProperty(nameof(BaseEntity.Id))!
            .SetValue(entity, id);
    }
}
