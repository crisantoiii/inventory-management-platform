using InventoryPlatform.Application.Interfaces.Authorization;
using Xunit;

namespace InventoryPlatform.Web.Tests.Authorization;

/// <summary>
/// Verifies that <see cref="FakeCapabilityAuthorizationService"/> correctly
/// implements <see cref="ICapabilityAuthorizationService"/> and produces
/// controlled, deterministic authorization results for T03/T04 handler tests.
/// </summary>
public class FakeCapabilityAuthorizationServiceTests
{
    private static readonly Guid TestUser = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA");
    private const string TestCapability = "Product.View";

    [Fact]
    public async Task ImplementsICapabilityAuthorizationService()
    {
        var fake = new FakeCapabilityAuthorizationService();

        Assert.IsAssignableFrom<ICapabilityAuthorizationService>(fake);
    }

    [Fact]
    public async Task CanBeInstantiated()
    {
        var fake = new FakeCapabilityAuthorizationService();

        var result = await fake.HasCapabilityAsync(TestUser, TestCapability);

        Assert.False(result); // default is false
    }

    [Fact]
    public async Task SetResult_WhenConfigured_ReturnsConfiguredResult()
    {
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetResult(TestUser, TestCapability, true);

        var result = await fake.HasCapabilityAsync(TestUser, TestCapability);

        Assert.True(result);
    }

    [Fact]
    public async Task SetResult_WhenDifferentUser_ReturnsDefaultResult()
    {
        var otherUser = Guid.Parse("BBBBBBBB-BBBB-BBBB-BBBB-BBBBBBBBBBBB");
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetResult(TestUser, TestCapability, true);

        var result = await fake.HasCapabilityAsync(otherUser, TestCapability);

        Assert.False(result);
    }

    [Fact]
    public async Task SetResult_WhenDifferentCapability_ReturnsDefaultResult()
    {
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetResult(TestUser, TestCapability, true);

        var result = await fake.HasCapabilityAsync(TestUser, "Category.Create");

        Assert.False(result);
    }

    [Fact]
    public async Task SetDefaultResult_ReturnsDefaultForUnconfigured()
    {
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetDefaultResult(true);

        var result = await fake.HasCapabilityAsync(TestUser, TestCapability);

        Assert.True(result);
    }

    [Fact]
    public async Task TracksLastRequestedUserId()
    {
        var fake = new FakeCapabilityAuthorizationService();

        await fake.HasCapabilityAsync(TestUser, TestCapability);

        Assert.Equal(TestUser, fake.LastRequestedUserId);
    }

    [Fact]
    public async Task TracksLastRequestedCapabilityName()
    {
        var fake = new FakeCapabilityAuthorizationService();

        await fake.HasCapabilityAsync(TestUser, TestCapability);

        Assert.Equal(TestCapability, fake.LastRequestedCapabilityName);
    }

    [Fact]
    public async Task TracksCallCount()
    {
        var fake = new FakeCapabilityAuthorizationService();

        await fake.HasCapabilityAsync(TestUser, TestCapability);
        await fake.HasCapabilityAsync(TestUser, "Category.Create");

        Assert.Equal(2, fake.CallCount);
    }

    [Fact]
    public async Task Reset_ClearsAllState()
    {
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetResult(TestUser, TestCapability, true);
        fake.SetDefaultResult(true);

        await fake.HasCapabilityAsync(TestUser, TestCapability);
        fake.Reset();

        var result = await fake.HasCapabilityAsync(TestUser, TestCapability);

        Assert.False(result);
        Assert.Equal(1, fake.CallCount);
    }

    [Fact]
    public async Task SetResultForCapability_AppliesToAllUsers()
    {
        var otherUser = Guid.Parse("CCCCCCCC-CCCC-CCCC-CCCC-CCCCCCCCCCCC");
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetResultForCapability(TestCapability, true);

        var result1 = await fake.HasCapabilityAsync(TestUser, TestCapability);
        var result2 = await fake.HasCapabilityAsync(otherUser, TestCapability);

        Assert.True(result1);
        Assert.True(result2);
    }

    [Fact]
    public async Task MultipleCapabilities_CanBeConfiguredIndependently()
    {
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetResult(TestUser, "Product.View", true);
        fake.SetResult(TestUser, "Product.Create", false);

        var viewResult = await fake.HasCapabilityAsync(TestUser, "Product.View");
        var createResult = await fake.HasCapabilityAsync(TestUser, "Product.Create");

        Assert.True(viewResult);
        Assert.False(createResult);
    }
}
