using InventoryPlatform.Domain.Entities;
using Xunit;

namespace InventoryPlatform.UnitTests.Domain.Authorization;

public class CapabilityTests
{
    [Fact]
    public void Constructor_WithValidName_CreatesCapability()
    {
        var capability = new Capability("Product.View");

        Assert.True(capability.Id >= 0);
        Assert.Equal("Product.View", capability.Name);
        Assert.True(capability.IsEnabled);
        Assert.Empty(capability.GroupCapabilities);
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Capability(string.Empty));
    }

    [Fact]
    public void Constructor_WithNullName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Capability(null!));
    }

    [Fact]
    public void Constructor_WithWhitespaceName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Capability("   "));
    }

    [Fact]
    public void Constructor_DefaultState_IsEnabledIsTrue()
    {
        var capability = new Capability("Test.Capability");

        Assert.True(capability.IsEnabled);
    }

    [Fact]
    public void Rename_WithValidName_UpdatesName()
    {
        var capability = new Capability("Old.Name");

        capability.Rename("New.Name");

        Assert.Equal("New.Name", capability.Name);
    }

    [Fact]
    public void Rename_WithEmptyName_ThrowsArgumentException()
    {
        var capability = new Capability("Valid.Name");

        Assert.Throws<ArgumentException>(() => capability.Rename(string.Empty));
    }

    [Fact]
    public void Rename_WithNullName_ThrowsArgumentException()
    {
        var capability = new Capability("Valid.Name");

        Assert.Throws<ArgumentException>(() => capability.Rename(null!));
    }

    [Fact]
    public void Rename_DoesNotAffectIsEnabled()
    {
        var capability = new Capability("Original");
        capability.Disable();

        capability.Rename("Renamed");

        Assert.False(capability.IsEnabled);
    }

    [Fact]
    public void Enable_WhenDisabled_EnablesCapability()
    {
        var capability = new Capability("Test");
        capability.Disable();

        capability.Enable();

        Assert.True(capability.IsEnabled);
    }

    [Fact]
    public void Enable_WhenAlreadyEnabled_KeepsEnabled()
    {
        var capability = new Capability("Test");
        Assert.True(capability.IsEnabled);

        capability.Enable();

        Assert.True(capability.IsEnabled);
    }

    [Fact]
    public void Disable_WhenEnabled_DisablesCapability()
    {
        var capability = new Capability("Test");
        Assert.True(capability.IsEnabled);

        capability.Disable();

        Assert.False(capability.IsEnabled);
    }

    [Fact]
    public void Disable_WhenAlreadyDisabled_KeepsDisabled()
    {
        var capability = new Capability("Test");
        capability.Disable();

        capability.Disable();

        Assert.False(capability.IsEnabled);
    }

    [Fact]
    public void Disable_ThenEnable_ReturnsToEnabled()
    {
        var capability = new Capability("Test");

        capability.Disable();
        Assert.False(capability.IsEnabled);

        capability.Enable();
        Assert.True(capability.IsEnabled);
    }

    [Fact]
    public void GroupCapabilities_InitiallyEmpty()
    {
        var capability = new Capability("Test");

        Assert.NotNull(capability.GroupCapabilities);
        Assert.Empty(capability.GroupCapabilities);
    }

    [Fact]
    public void GroupCapabilities_IsReadOnly()
    {
        var capability = new Capability("Test");

        Assert.IsAssignableFrom<IReadOnlyCollection<AuthorizationGroupCapability>>(capability.GroupCapabilities);
    }
}
