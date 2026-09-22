using System.Net;
using System.Net.Http;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Web.Tests.Authentication;
using InventoryPlatform.Web.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InventoryPlatform.Web.Tests.Http;

/// <summary>
/// Sprint 17 T06 — Category Create real-antiforgery POST / PRG / persistence.
/// Exercises the real HTTPS Razor pipeline: rendered form action, rendered
/// antiforgery token with its matching preserved cookie, real model binding,
/// real authorization, real antiforgery validation, real
/// PageModel/Application handling, the actual PRG redirect, and
/// same-factory InMemory persistence. Antiforgery is not disabled,
/// bypassed, or replaced; the token is never manufactured and no handler is
/// invoked directly.
/// </summary>
public sealed class CategoryCreatePostWorkflowTests
{
    private const string CategoryCreatePath = "/Categories/Create";

    // Production behavior (verified from the real pipeline): the Create
    // form posts to the current URL and successful create redirects to
    // /Categories ( RedirectToPage("Index") resolves to /Categories ).
    private const string CategoryIndexPath = "/Categories";

    private const string AntiforgeryCookieName = "InventoryPlatform.AntiForgery";

    private const string AntiforgeryTokenFieldName = "__RequestVerificationToken";

    [Fact]
    public async Task PostCategoryCreate_AsInventoryManager_WithRenderedAntiforgeryMaterial_PersistsCategoryAndRedirects()
    {
        // Arrange — real factory, real seeded InventoryManager selector,
        // cookie-preserving client so the antiforgery cookie from GET is
        // sent with the POST.
        await using var factory = new InventoryPlatformWebApplicationFactory();

        var cookieContainer = new CookieContainer();

        using var client = CreateClient(factory, cookieContainer);

        // Act — GET the real rendered Create page.
        var getResponse = await client.GetAsync(CategoryCreatePath);
        var getHtml = await getResponse.Content.ReadAsStringAsync();

        Assert.True(
            getResponse.StatusCode == HttpStatusCode.OK,
            $"Expected HTTP 200 on GET {CategoryCreatePath} but received " +
            $"{(int)getResponse.StatusCode}.{Environment.NewLine}{getHtml}");

        // Prove the real form is rendered.
        Assert.Contains(
            "name=\"Category.Name\"",
            getHtml,
            StringComparison.Ordinal);

        // Consume the actual rendered form action.
        var formAction = CategoryCreateFormExtraction
            .ExtractCreateFormAction(getHtml);

        Assert.Equal(CategoryCreatePath, formAction);

        // Consume the real rendered antiforgery hidden token.
        var antiforgeryToken = CategoryCreateFormExtraction
            .ExtractCreateFormAntiforgeryToken(getHtml);

        Assert.False(
            string.IsNullOrWhiteSpace(antiforgeryToken),
            "The rendered antiforgery token must not be empty.");

        // Prove the matching antiforgery cookie was preserved by the client.
        var preservedAntiforgeryCookie = GetAntiforgeryCookie(
            cookieContainer,
            client.BaseAddress!);

        Assert.False(
            string.IsNullOrWhiteSpace(preservedAntiforgeryCookie.Value),
            "The antiforgery cookie emitted by the GET response must have a value.");

        // Act — POST to the rendered form action with the real token and a
        // unique valid Category.
        var categoryName = $"T06 Category {Guid.NewGuid():N}";
        var categoryDescription = $"T06 description {Guid.NewGuid():N}";

        using var postRequest = new HttpRequestMessage(
            HttpMethod.Post,
            formAction)
        {
            Content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    [AntiforgeryTokenFieldName] = antiforgeryToken,
                    ["Category.Name"] = categoryName,
                    ["Category.Description"] = categoryDescription
                })
        };

        var postResponse = await client.SendAsync(postRequest);

        var postHtml = await postResponse.Content.ReadAsStringAsync();

        // Assert — the actual successful PRG response from the POST itself.
        Assert.True(
            postResponse.StatusCode == HttpStatusCode.Redirect,
            $"Expected HTTP 302 (Post-Redirect-Get) but received " +
            $"{(int)postResponse.StatusCode}.{Environment.NewLine}{postHtml}");

        var location = Assert.IsType<Uri>(postResponse.Headers.Location);
        var redirectDestination = location.IsAbsoluteUri
            ? location
            : new Uri(client.BaseAddress!, location);

        Assert.Equal(CategoryIndexPath, redirectDestination.AbsolutePath);

        // Assert — persistence in the same isolated factory database via a
        // fresh DI scope from the same factory.
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var persistedCategory = await dbContext.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(
                category => category.Name == categoryName);

        Assert.NotNull(persistedCategory);
        Assert.Equal(categoryName, persistedCategory!.Name);
        Assert.Equal(categoryDescription, persistedCategory.Description);
        Assert.True(persistedCategory.IsActive);
    }

    private static HttpClient CreateClient(
        InventoryPlatformWebApplicationFactory factory,
        CookieContainer cookieContainer)
    {
        // A narrow cookie-preserving handler wraps the factory's request-
        // creating handler: the antiforgery cookie emitted by the GET
        // response is preserved and sent with the POST, exactly as a browser
        // would. Redirects are not followed, so the initial POST PRG
        // response is observable. No antiforgery services are touched and
        // no token is manufactured.
        var cookieHandler = new CookiePreservingHandler(cookieContainer)
        {
            InnerHandler = factory.Server.CreateHandler()
        };

        var client = new HttpClient(cookieHandler)
        {
            BaseAddress = new Uri("https://localhost"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        // The real InventoryManager selector is kept across GET and POST.
        client.DefaultRequestHeaders.Add(
            TestAuthenticationDefaults.UserHeader,
            TestUserSelectors.InventoryManager);

        return client;
    }

    private static Cookie GetAntiforgeryCookie(
        CookieContainer cookieContainer,
        Uri baseAddress)
    {
        var antiforgeryCookies = cookieContainer
            .GetCookies(baseAddress)
            .Cast<Cookie>()
            .Where(cookie => cookie.Name == AntiforgeryCookieName)
            .ToList();

        return Assert.Single(antiforgeryCookies);
    }

    /// <summary>
    /// Narrow BCL cookie-preserving handler: replays preserved cookies onto
    /// outgoing requests and stores Set-Cookie headers from responses. No
    /// antiforgery, authentication, or redirect behavior.
    /// </summary>
    private sealed class CookiePreservingHandler : DelegatingHandler
    {
        private readonly CookieContainer _cookieContainer;

        public CookiePreservingHandler(CookieContainer cookieContainer)
        {
            _cookieContainer = cookieContainer;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var cookieHeader = _cookieContainer.GetCookieHeader(
                request.RequestUri!);

            if (!string.IsNullOrEmpty(cookieHeader))
            {
                request.Headers.TryAddWithoutValidation(
                    "Cookie",
                    cookieHeader);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.Headers.TryGetValues(
                    "Set-Cookie",
                    out var setCookieHeaders))
            {
                foreach (var setCookieHeader in setCookieHeaders)
                {
                    _cookieContainer.SetCookies(
                        request.RequestUri!,
                        setCookieHeader);
                }
            }

            return response;
        }
    }
}
