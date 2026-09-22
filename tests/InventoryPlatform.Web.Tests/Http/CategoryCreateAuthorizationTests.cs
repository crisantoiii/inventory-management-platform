using System.Net;
using InventoryPlatform.Web.Tests.Authentication;
using InventoryPlatform.Web.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Xunit;

namespace InventoryPlatform.Web.Tests.Http;

public sealed class CategoryCreateAuthorizationTests
{
    private const string CategoryCreatePath = "/Categories/Create";

    [Fact]
    public async Task GetCategoryCreate_WithoutTestUser_RedirectsToIdentityLogin()
    {
        await using var factory = new InventoryPlatformWebApplicationFactory();
        using var client = CreateClient(factory);

        var response = await client.GetAsync(CategoryCreatePath);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        AssertRedirect(
            client,
            response,
            expectedPath: "/Identity/Account/Login",
            expectedReturnUrl: CategoryCreatePath);
        Assert.False(client.DefaultRequestHeaders.Contains(
            TestAuthenticationDefaults.UserHeader));
    }

    [Fact]
    public async Task GetCategoryCreate_AsInventoryManager_RendersRazorPage()
    {
        await using var factory = new InventoryPlatformWebApplicationFactory();
        using var client = CreateClient(factory, TestUserSelectors.InventoryManager);

        var response = await client.GetAsync(CategoryCreatePath);
        var content = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected HTTP 200 but received {(int)response.StatusCode}.{Environment.NewLine}{content}");
        Assert.Contains("name=\"Category.Name\"", content, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetCategoryCreate_AsViewer_RedirectsToIdentityAccessDenied()
    {
        await using var factory = new InventoryPlatformWebApplicationFactory();
        using var client = CreateClient(factory, TestUserSelectors.Viewer);

        var response = await client.GetAsync(CategoryCreatePath);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        AssertRedirect(
            client,
            response,
            expectedPath: "/Identity/Account/AccessDenied",
            expectedReturnUrl: CategoryCreatePath);
        Assert.Equal(
            TestUserSelectors.Viewer,
            client.DefaultRequestHeaders.GetValues(
                TestAuthenticationDefaults.UserHeader).Single());
    }

    private static HttpClient CreateClient(
        InventoryPlatformWebApplicationFactory factory,
        string? selector = null)
    {
        var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost")
            });

        if (selector is not null)
        {
            client.DefaultRequestHeaders.Add(
                TestAuthenticationDefaults.UserHeader,
                selector);
        }

        return client;
    }

    private static void AssertRedirect(
        HttpClient client,
        HttpResponseMessage response,
        string expectedPath,
        string expectedReturnUrl)
    {
        var location = Assert.IsType<Uri>(response.Headers.Location);
        var destination = location.IsAbsoluteUri
            ? location
            : new Uri(client.BaseAddress!, location);
        var query = QueryHelpers.ParseQuery(destination.Query);

        Assert.Equal(expectedPath, destination.AbsolutePath);
        Assert.True(query.TryGetValue("ReturnUrl", out var returnUrl));
        Assert.Equal(expectedReturnUrl, Assert.Single(returnUrl));
    }
}
