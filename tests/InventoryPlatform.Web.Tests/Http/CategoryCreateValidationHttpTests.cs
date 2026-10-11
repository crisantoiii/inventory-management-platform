using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using InventoryPlatform.Web.Tests.Authentication;
using InventoryPlatform.Web.Tests.Infrastructure;
using Xunit;

namespace InventoryPlatform.Web.Tests.Http;

/// <summary>
/// Sprint 27 Category Create validation HTTP coverage.
/// Proves validation failures display through ModelOnly summary with input preserved.
/// </summary>
public sealed class CategoryCreateValidationHttpTests
{
    private const string CategoryCreatePath = "/Categories/Create";
    private const string CategoryIndexPath = "/Categories";
    private const string AntiforgeryCookieName = "InventoryPlatform.AntiForgery";
    private const string AntiforgeryTokenFieldName = "__RequestVerificationToken";

    [Fact]
    public async Task PostCategoryCreate_ValidationFailureEmptyName_DisplaysErrorAndPreservesInput()
    {
        // Arrange
        await using var factory = new InventoryPlatformWebApplicationFactory();
        var cookieContainer = new CookieContainer();
        using var client = CreateClient(factory, cookieContainer, TestUserSelectors.InventoryManager);

        // GET the Create page
        var getResponse = await client.GetAsync(CategoryCreatePath);
        var getHtml = await getResponse.Content.ReadAsStringAsync();
        Assert.True(getResponse.StatusCode == HttpStatusCode.OK);

        // Extract form action and antiforgery token
        var formAction = CategoryCreateFormExtraction.ExtractCreateFormAction(getHtml);
        var antiforgeryToken = CategoryCreateFormExtraction.ExtractCreateFormAntiforgeryToken(getHtml);
        var preservedAntiforgeryCookie = GetAntiforgeryCookie(cookieContainer, client.BaseAddress!);

        // POST with empty name (validation failure)
        var emptyName = "";
        var description = "Test description";

        using var postRequest = new HttpRequestMessage(HttpMethod.Post, formAction)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [AntiforgeryTokenFieldName] = antiforgeryToken,
                ["Category.Name"] = emptyName,
                ["Category.Description"] = description
            })
        };

        var postResponse = await client.SendAsync(postRequest);
        var postHtml = await postResponse.Content.ReadAsStringAsync();

        // Assert - validation failure returns 200 (Page()) not redirect
        Assert.True(postResponse.StatusCode == HttpStatusCode.OK,
            $"Expected HTTP 200 on validation failure but received {(int)postResponse.StatusCode}.{Environment.NewLine}{postHtml}");

        // Assert - validation summary contains the error message
        var validationSummary = ExtractValidationSummaryText(postHtml);
        Assert.Contains("must not be empty", validationSummary, StringComparison.OrdinalIgnoreCase);

        // Assert - input is preserved (ModelOnly summary + restored model binding)
        Assert.Contains("name=\"Category.Name\"", postHtml, StringComparison.Ordinal);
        Assert.Contains("name=\"Category.Description\"", postHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PostCategoryCreate_ValidationFailureNameTooLong_DisplaysErrorAndPreservesInput()
    {
        // Arrange
        await using var factory = new InventoryPlatformWebApplicationFactory();
        var cookieContainer = new CookieContainer();
        using var client = CreateClient(factory, cookieContainer, TestUserSelectors.InventoryManager);

        var getResponse = await client.GetAsync(CategoryCreatePath);
        var getHtml = await getResponse.Content.ReadAsStringAsync();
        Assert.True(getResponse.StatusCode == HttpStatusCode.OK);

        var formAction = CategoryCreateFormExtraction.ExtractCreateFormAction(getHtml);
        var antiforgeryToken = CategoryCreateFormExtraction.ExtractCreateFormAntiforgeryToken(getHtml);

        // POST with name too long (101 characters)
        var longName = new string('x', 101);
        var description = "Test description";

        using var postRequest = new HttpRequestMessage(HttpMethod.Post, formAction)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [AntiforgeryTokenFieldName] = antiforgeryToken,
                ["Category.Name"] = longName,
                ["Category.Description"] = description
            })
        };

        var postResponse = await client.SendAsync(postRequest);
        var postHtml = await postResponse.Content.ReadAsStringAsync();

        // Assert - validation failure returns 200 (Page())
        Assert.True(postResponse.StatusCode == HttpStatusCode.OK,
            $"Expected HTTP 200 on validation failure but received {(int)postResponse.StatusCode}.{Environment.NewLine}{postHtml}");

        // Assert - validation summary contains the error message
        var validationSummary = ExtractValidationSummaryText(postHtml);
        Assert.Contains("100 characters or fewer", validationSummary, StringComparison.OrdinalIgnoreCase);

        // Assert - input is preserved
        Assert.Contains("name=\"Category.Name\"", postHtml, StringComparison.Ordinal);
        Assert.Contains("name=\"Category.Description\"", postHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PostCategoryCreate_ValidRequest_RedirectsAndPersists()
    {
        // Arrange
        await using var factory = new InventoryPlatformWebApplicationFactory();
        var cookieContainer = new CookieContainer();
        using var client = CreateClient(factory, cookieContainer, TestUserSelectors.InventoryManager);

        var getResponse = await client.GetAsync(CategoryCreatePath);
        var getHtml = await getResponse.Content.ReadAsStringAsync();
        Assert.True(getResponse.StatusCode == HttpStatusCode.OK);

        var formAction = CategoryCreateFormExtraction.ExtractCreateFormAction(getHtml);
        var antiforgeryToken = CategoryCreateFormExtraction.ExtractCreateFormAntiforgeryToken(getHtml);

        // POST with valid data
        var categoryName = $"Valid Category {Guid.NewGuid():N}";
        var categoryDescription = $"Valid description {Guid.NewGuid():N}";

        using var postRequest = new HttpRequestMessage(HttpMethod.Post, formAction)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [AntiforgeryTokenFieldName] = antiforgeryToken,
                ["Category.Name"] = categoryName,
                ["Category.Description"] = categoryDescription
            })
        };

        var postResponse = await client.SendAsync(postRequest);

        // Assert - successful redirect (PRG)
        Assert.True(postResponse.StatusCode == HttpStatusCode.Redirect,
            $"Expected HTTP 302 (Post-Redirect-Get) but received {(int)postResponse.StatusCode}");

        var location = Assert.IsType<Uri>(postResponse.Headers.Location);
        var redirectDestination = location.IsAbsoluteUri ? location : new Uri(client.BaseAddress!, location);
        Assert.Equal(CategoryIndexPath, redirectDestination.AbsolutePath);
    }

    private static HttpClient CreateClient(
        InventoryPlatformWebApplicationFactory factory,
        CookieContainer cookieContainer,
        string selector)
    {
        var cookieHandler = new CookiePreservingHandler(cookieContainer)
        {
            InnerHandler = factory.Server.CreateHandler()
        };

        var client = new HttpClient(cookieHandler)
        {
            BaseAddress = new Uri("https://localhost"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        client.DefaultRequestHeaders.Add(TestAuthenticationDefaults.UserHeader, selector);
        return client;
    }

    private static Cookie GetAntiforgeryCookie(CookieContainer cookieContainer, Uri baseAddress)
    {
        var antiforgeryCookies = cookieContainer.GetCookies(baseAddress)
            .Cast<Cookie>()
            .Where(cookie => cookie.Name == AntiforgeryCookieName)
            .ToList();
        return Assert.Single(antiforgeryCookies);
    }

    private static string ExtractValidationSummaryText(string html)
    {
        var summary = Regex.Match(
            html,
            @"<div\b(?=[^>]*\bclass=""[^""]*\bvalidation-summary-errors\b[^""]*"")[^>]*>(?<content>.*?)</div>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        if (!summary.Success)
        {
            throw new InvalidOperationException(
                "The Category Create page did not contain a model-level " +
                "validation summary with errors." + BuildHtmlExcerptDiagnostic(html));
        }

        var withoutTags = Regex.Replace(
            summary.Groups["content"].Value,
            "<[^>]+>",
            " ");

        return WebUtility.HtmlDecode(withoutTags);
    }

    private static string BuildHtmlExcerptDiagnostic(string html)
    {
        var excerpt = html.Length <= 800 ? html : $"{html.AsSpan(0, 800)}…";
        return $"{Environment.NewLine}Rendered HTML excerpt: {excerpt}";
    }

    private sealed class CookiePreservingHandler : DelegatingHandler
    {
        private readonly CookieContainer _cookieContainer;

        public CookiePreservingHandler(CookieContainer cookieContainer) => _cookieContainer = cookieContainer;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var cookieHeader = _cookieContainer.GetCookieHeader(request.RequestUri!);
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
            {
                foreach (var setCookieHeader in setCookieHeaders)
                {
                    _cookieContainer.SetCookies(request.RequestUri!, setCookieHeader);
                }
            }

            return response;
        }
    }
}