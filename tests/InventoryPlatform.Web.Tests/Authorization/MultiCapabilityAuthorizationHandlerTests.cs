using System.Security.Claims;
using InventoryPlatform.Application.Interfaces.Authorization;
using InventoryPlatform.Web.Authorization;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace InventoryPlatform.Web.Tests.Authorization;

/// <summary>
/// Verifies the actual behavior of <see cref="MultiCapabilityAuthorizationHandler"/>:
/// OR semantics across the requirement's capability list (first granted capability
/// satisfies the requirement), authentication gate, NameIdentifier claim extraction
/// and GUID parsing, delegation to <see cref="ICapabilityAuthorizationService"/>,
/// and succeed/do-not-succeed outcomes. Also verifies
/// <see cref="MultiCapabilityRequirement"/> construction validation.
/// </summary>
public class MultiCapabilityAuthorizationHandlerTests
{
    private static readonly Guid AuthorizedUser = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherUser = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private const string FirstCapability = "Product.View";
    private const string SecondCapability = "Category.View";
    private const string ThirdCapability = "Unit.View";

    [Fact]
    public async Task HandleAsync_WhenFirstCapabilitySucceeds_SucceedsRequirement()
    {
        // OR semantics: a single granted capability is sufficient, and the
        // handler must stop evaluating once one capability succeeds.
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetResult(AuthorizedUser, FirstCapability, true);
        fake.SetResult(AuthorizedUser, SecondCapability, false);
        var user = CreateUserWithNameIdentifier(AuthorizedUser);

        var context = await HandleAsync(fake, user, FirstCapability, SecondCapability);

        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
        Assert.Equal(1, fake.CallCount);
        Assert.Equal(AuthorizedUser, fake.LastRequestedUserId);
        Assert.Equal(FirstCapability, fake.LastRequestedCapabilityName);
    }

    [Fact]
    public async Task HandleAsync_WhenSecondCapabilitySucceeds_SucceedsRequirement()
    {
        // The handler must not stop at the first capability: the second one
        // satisfies the OR requirement even though the first was denied.
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetResult(AuthorizedUser, FirstCapability, false);
        fake.SetResult(AuthorizedUser, SecondCapability, true);
        var user = CreateUserWithNameIdentifier(AuthorizedUser);

        var context = await HandleAsync(fake, user, FirstCapability, SecondCapability);

        Assert.True(context.HasSucceeded);
        Assert.False(context.HasFailed);
        Assert.Equal(2, fake.CallCount);
        Assert.Equal(SecondCapability, fake.LastRequestedCapabilityName);
    }

    [Fact]
    public async Task HandleAsync_WhenLaterCapabilitySucceeds_SucceedsRequirement()
    {
        // OR semantics over more than two capabilities: the third capability
        // satisfies the requirement after the first two were denied.
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetResult(AuthorizedUser, FirstCapability, false);
        fake.SetResult(AuthorizedUser, SecondCapability, false);
        fake.SetResult(AuthorizedUser, ThirdCapability, true);
        var user = CreateUserWithNameIdentifier(AuthorizedUser);

        var context = await HandleAsync(fake, user, FirstCapability, SecondCapability, ThirdCapability);

        Assert.True(context.HasSucceeded);
        Assert.Equal(3, fake.CallCount);
        Assert.Equal(ThirdCapability, fake.LastRequestedCapabilityName);
    }

    [Fact]
    public async Task HandleAsync_WhenAllCapabilitiesDenied_DoesNotSucceedRequirement()
    {
        // Every capability is evaluated; none being granted means the OR
        // requirement is not satisfied. The context neither succeeds nor fails
        // (failure is left to the authorization pipeline).
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetResult(AuthorizedUser, FirstCapability, false);
        fake.SetResult(AuthorizedUser, SecondCapability, false);
        fake.SetResult(AuthorizedUser, ThirdCapability, false);
        var user = CreateUserWithNameIdentifier(AuthorizedUser);

        var context = await HandleAsync(fake, user, FirstCapability, SecondCapability, ThirdCapability);

        Assert.False(context.HasSucceeded);
        Assert.False(context.HasFailed);
        Assert.Equal(3, fake.CallCount);
    }

    [Fact]
    public async Task HandleAsync_WhenCapabilitiesGrantedOnlyToAnotherUser_DoesNotSucceedRequirement()
    {
        // The grants are keyed to a different user's GUID, so the authenticated
        // principal does not hold them. This proves the handler asks the service
        // for the principal's own user ID and does not succeed on other users' grants.
        var fake = new FakeCapabilityAuthorizationService();
        fake.SetResult(OtherUser, FirstCapability, true);
        fake.SetResult(OtherUser, SecondCapability, true);
        var user = CreateUserWithNameIdentifier(AuthorizedUser);

        var context = await HandleAsync(fake, user, FirstCapability, SecondCapability);

        Assert.False(context.HasSucceeded);
        Assert.False(context.HasFailed);
        Assert.Equal(2, fake.CallCount);
        Assert.Equal(AuthorizedUser, fake.LastRequestedUserId);
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

        var context = await HandleAsync(fake, user, FirstCapability, SecondCapability);

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

        var context = await HandleAsync(fake, user, FirstCapability, SecondCapability);

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

        var context = await HandleAsync(fake, user, FirstCapability, SecondCapability);

        Assert.False(context.HasSucceeded);
        Assert.Equal(0, fake.CallCount);
        Assert.Null(fake.LastRequestedUserId);
    }

    [Fact]
    public void Constructor_WithValidCapabilityNames_SetsCapabilityNamesInOrder()
    {
        var requirement = new MultiCapabilityRequirement(
            FirstCapability, SecondCapability, ThirdCapability);

        Assert.Equal(
            new[] { FirstCapability, SecondCapability, ThirdCapability },
            requirement.CapabilityNames);
    }

    [Fact]
    public void Constructor_WhenCapabilityNamesIsNull_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new MultiCapabilityRequirement(null!));
    }

    [Fact]
    public void Constructor_WhenCapabilityNamesIsEmpty_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new MultiCapabilityRequirement());
    }

    [Fact]
    public void Constructor_WhenCapabilityNameIsWhitespace_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new MultiCapabilityRequirement(FirstCapability, "   "));
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
        params string[] capabilityNames)
    {
        var requirement = new MultiCapabilityRequirement(capabilityNames);
        var context = new AuthorizationHandlerContext(
            new IAuthorizationRequirement[] { requirement },
            user,
            resource: null);

        var handler = new MultiCapabilityAuthorizationHandler(fake);
        await handler.HandleAsync(context);

        return context;
    }
}