using System.Security.Claims;
using InventoryPlatform.Infrastructure.Identity;
using InventoryPlatform.Web.Authorization;
using InventoryPlatform.Web.Tests.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using AspNetIdentityConstants = Microsoft.AspNetCore.Identity.IdentityConstants;

namespace InventoryPlatform.Web.Tests.Authentication;

public sealed class SeededUserAuthenticationTests
{
    [Fact]
    public async Task AuthenticateAsync_WithoutSelector_ReturnsNoResult()
    {
        await using var factory = new InventoryPlatformWebApplicationFactory();
        await using var scope = factory.Services.CreateAsyncScope();
        var context = CreateHttpContext(scope.ServiceProvider);

        var result = await context.AuthenticateAsync();

        Assert.True(result.None);
        Assert.Null(result.Principal);
        Assert.False(context.User.Identity?.IsAuthenticated ?? false);
    }

    [Theory]
    [InlineData(TestUserSelectors.Administrator)]
    [InlineData(TestUserSelectors.InventoryManager)]
    [InlineData(TestUserSelectors.Viewer)]
    public async Task AuthenticateAsync_WithSeededSelector_UsesRealSeededGuid(
        string selector)
    {
        await using var factory = new InventoryPlatformWebApplicationFactory();
        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();
        var expectedUser = await userManager.FindByEmailAsync(selector);
        var context = CreateHttpContext(scope.ServiceProvider, selector);

        var result = await context.AuthenticateAsync();

        Assert.NotNull(expectedUser);
        Assert.True(result.Succeeded);
        Assert.Equal(
            expectedUser.Id.ToString(),
            result.Principal!.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal(
            expectedUser.UserName,
            result.Principal.FindFirstValue(ClaimTypes.Name));
        Assert.True(result.Principal.Identity!.IsAuthenticated);
    }

    [Fact]
    public async Task AuthenticateAsync_WithUnknownSelector_FailsWithoutPrincipal()
    {
        await using var factory = new InventoryPlatformWebApplicationFactory();
        await using var scope = factory.Services.CreateAsyncScope();
        var context = CreateHttpContext(
            scope.ServiceProvider,
            "unknown@inventory.invalid");

        var result = await context.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.False(result.None);
        Assert.NotNull(result.Failure);
        Assert.Null(result.Principal);
    }

    [Fact]
    public async Task AuthenticateAsync_WithSeededSelector_DoesNotFabricateAuthorizationClaims()
    {
        await using var factory = new InventoryPlatformWebApplicationFactory();
        await using var scope = factory.Services.CreateAsyncScope();
        var context = CreateHttpContext(
            scope.ServiceProvider,
            TestUserSelectors.InventoryManager);

        var result = await context.AuthenticateAsync();

        Assert.True(result.Succeeded);
        Assert.DoesNotContain(
            result.Principal!.Claims,
            claim => claim.Type == ClaimTypes.Role);
        Assert.DoesNotContain(
            result.Principal.Claims,
            claim => claim.Type.Contains("capability", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(2, result.Principal.Claims.Count());
    }

    [Fact]
    public async Task AuthorizeAsync_UsesRealSeededCapabilityRelationships()
    {
        await using var factory = new InventoryPlatformWebApplicationFactory();
        await using var managerScope = factory.Services.CreateAsyncScope();
        var managerResult = await AuthenticateAndAuthorizeAsync(
            managerScope.ServiceProvider,
            TestUserSelectors.InventoryManager);
        await using var viewerScope = factory.Services.CreateAsyncScope();
        var viewerResult = await AuthenticateAndAuthorizeAsync(
            viewerScope.ServiceProvider,
            TestUserSelectors.Viewer);

        Assert.True(managerResult.Succeeded);
        Assert.False(viewerResult.Succeeded);
    }

    [Fact]
    public async Task AuthenticationOptions_PreserveIdentityChallengeAndForbidSchemes()
    {
        await using var factory = new InventoryPlatformWebApplicationFactory();
        var schemeProvider = factory.Services
            .GetRequiredService<IAuthenticationSchemeProvider>();
        var authenticateScheme = await schemeProvider.GetDefaultAuthenticateSchemeAsync();
        var challengeScheme = await schemeProvider.GetDefaultChallengeSchemeAsync();
        var forbidScheme = await schemeProvider.GetDefaultForbidSchemeAsync();

        Assert.Equal(TestAuthenticationDefaults.Scheme, authenticateScheme!.Name);
        Assert.Equal(AspNetIdentityConstants.ApplicationScheme, challengeScheme!.Name);
        Assert.Equal(AspNetIdentityConstants.ApplicationScheme, forbidScheme!.Name);
    }

    private static DefaultHttpContext CreateHttpContext(
        IServiceProvider serviceProvider,
        string? selector = null)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        if (selector is not null)
        {
            context.Request.Headers[TestAuthenticationDefaults.UserHeader] = selector;
        }

        return context;
    }

    private static async Task<ClaimsPrincipal> AuthenticateAsync(
        IServiceProvider serviceProvider,
        string selector)
    {
        var context = CreateHttpContext(serviceProvider, selector);
        var result = await context.AuthenticateAsync();

        Assert.True(result.Succeeded);
        return result.Principal!;
    }

    private static async Task<AuthorizationResult> AuthenticateAndAuthorizeAsync(
        IServiceProvider serviceProvider,
        string selector)
    {
        var principal = await AuthenticateAsync(serviceProvider, selector);
        var authorizationService = serviceProvider
            .GetRequiredService<IAuthorizationService>();

        return await authorizationService.AuthorizeAsync(
            principal,
            resource: null,
            AuthorizationPolicies.InventoryManagement);
    }
}
