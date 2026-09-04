using System.Security.Claims;
using InventoryPlatform.Application.Interfaces.Authorization;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace InventoryPlatform.Web.Tests.Authorization;

/// <summary>
/// Verifies the actual behavior of <see cref="CapabilityAuthorizationHandler"/>:
/// authentication gate, NameIdentifier claim extraction and GUID parsing,
/// delegation to <see cref="ICapabilityAuthorizationService"/>,
/// and succeed/do-not-succeed outcomes.
/// </summary>
public class CapabilityAuthorizationHandlerTests
{
    private static readonly Guid AuthorizedUser = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherUser = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private const string GrantedCapability = "Product.View";
    private const string OtherCapability = "Category.Create";

    [Fact]
    public async Task HandleAsync_WhenUserHasRequiredCapability_SucceedsRequirement()
    {
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetResult(AuthorizedUser, GrantedCapability, true);
        var user = CreateUserWithNameIdentifier(AuthorizedUser);

        var context = await HandleAsync(fake, user, GrantedCapability);

        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
        Assert.Equal(1, fake.CallCount);
    }

    [Fact]
    public async Task HandleAsync_WhenUserLacksRequiredCapability_DoesNotSucceedRequirement()
    {
        // The capability is granted only to a different user, so the
        // authenticated user does not have it.
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetResult(OtherUser, GrantedCapability, true);
        var user = CreateUserWithNameIdentifier(AuthorizedUser);

        var context = await HandleAsync(fake, user, GrantedCapability);

        Assert.False(context.HasSucceeded);
        Assert.False(context.HasFailed);
        Assert.Equal(1, fake.CallCount);
        Assert.Equal(AuthorizedUser, fake.LastRequestedUserId);
    }

    [Fact]
    public async Task HandleAsync_WhenAuthorizationServiceReturnsFalse_DoesNotSucceedRequirement()
    {
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetResult(AuthorizedUser, GrantedCapability, false);
        var user = CreateUserWithNameIdentifier(AuthorizedUser);

        var context = await HandleAsync(fake, user, GrantedCapability);

        Assert.False(context.HasSucceeded);
        Assert.Equal(1, fake.CallCount);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIsAuthenticated_PassesNameIdentifierClaimValueAsUserId()
    {
        // The authorization grant is keyed to the GUID held in the principal's
        // NameIdentifier claim, so success proves the handler passed exactly
        // that claim value to the service.
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetResult(AuthorizedUser, GrantedCapability, true);
        var user = CreateUserWithNameIdentifier(AuthorizedUser);

        var context = await HandleAsync(fake, user, GrantedCapability);

        Assert.True(context.HasSucceeded);
        Assert.Equal(AuthorizedUser, fake.LastRequestedUserId);
        Assert.Equal(GrantedCapability, fake.LastRequestedCapabilityName);
    }

    [Fact]
    public async Task HandleAsync_WhenRequirementIsForAnotherCapability_RequestsRequirementCapabilityAndDoesNotSucceed()
    {
        // The user holds GrantedCapability but the requirement demands
        // OtherCapability; the handler must ask for the requirement's capability
        // and must not succeed on the unrelated grant.
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetResult(AuthorizedUser, GrantedCapability, true);
        var user = CreateUserWithNameIdentifier(AuthorizedUser);

        var context = await HandleAsync(fake, user, OtherCapability);

        Assert.False(context.HasSucceeded);
        Assert.Equal(1, fake.CallCount);
        Assert.Equal(OtherCapability, fake.LastRequestedCapabilityName);
    }

    [Fact]
    public async Task HandleAsync_WhenIdentityIsNotAuthenticated_DoesNotSucceedAndDoesNotCallService()
    {
        // The identity carries a valid NameIdentifier claim, but is not
        // authenticated; the handler must return before consulting the service.
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetDefaultResult(true);
        var user = CreateUnauthenticatedUser(
            new Claim(ClaimTypes.NameIdentifier, AuthorizedUser.ToString()));

        var context = await HandleAsync(fake, user, GrantedCapability);

        Assert.False(context.HasSucceeded);
        Assert.Equal(0, fake.CallCount);
        Assert.Null(fake.LastRequestedUserId);
        Assert.Null(fake.LastRequestedCapabilityName);
    }

    [Fact]
    public async Task HandleAsync_WhenNameIdentifierClaimIsMissing_DoesNotSucceedAndDoesNotCallService()
    {
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetDefaultResult(true);
        var user = CreateAuthenticatedUser();

        var context = await HandleAsync(fake, user, GrantedCapability);

        Assert.False(context.HasSucceeded);
        Assert.Equal(0, fake.CallCount);
        Assert.Null(fake.LastRequestedUserId);
    }

    [Fact]
    public async Task HandleAsync_WhenNameIdentifierIsNotAValidGuid_DoesNotSucceedAndDoesNotCallService()
    {
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetDefaultResult(true);
        var user = CreateAuthenticatedUser(
            new Claim(ClaimTypes.NameIdentifier, "not-a-valid-guid"));

        var context = await HandleAsync(fake, user, GrantedCapability);

        Assert.False(context.HasSucceeded);
        Assert.Equal(0, fake.CallCount);
        Assert.Null(fake.LastRequestedUserId);
    }

    private static ClaimsPrincipal CreateUserWithNameIdentifier(Guid userId)
    {
        return CreateAuthenticatedUser(
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
    }

    private static ClaimsPrincipal CreateAuthenticatedUser(params Claim[] claims)
    {
        return new ClaimsPrincipal(
            new ClaimsIdentity(claims, authenticationType: "TestAuthentication"));
    }

    private static ClaimsPrincipal CreateUnauthenticatedUser(params Claim[] claims)
    {
        // No authentication type => Identity.IsAuthenticated is false.
        return new ClaimsPrincipal(new ClaimsIdentity(claims));
    }

    private static async Task<AuthorizationHandlerContext> HandleAsync(
        FakeCapabilityAuthorizationService fake,
        ClaimsPrincipal user,
        string capabilityName)
    {
        var requirement = new CapabilityRequirement(capabilityName);
        var context = new AuthorizationHandlerContext(
            new IAuthorizationRequirement[] { requirement },
            user,
            resource: null);

        var handler = new CapabilityAuthorizationHandler(fake);
        await handler.HandleAsync(context);

        return context;
    }
}
