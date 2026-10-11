using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Web.Tests.Authentication;
using InventoryPlatform.Web.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InventoryPlatform.Web.Tests.Http;

/// <summary>
/// Sprint 27 Category Edit validation HTTP coverage.
/// Proves validation failures display through ModelOnly summary with input preserved.
/// </summary>
public sealed class CategoryEditValidationHttpTests
{
    private const string CategoryIndexPath = "/Categories";
    private const string CategoryEditPathPrefix = "/Categories/Edit/";
    private const string AntiforgeryCookieName = "InventoryPlatform.AntiForgery";
    private const string AntiforgeryTokenFieldName = "__RequestVerificationToken";

    [Fact]
    public async Task PostCategoryEdit_ValidationFailureEmptyName_DisplaysErrorAndPreservesInput()
    {
        // Arrange - create a category first
        await using var factory = new InventoryPlatformWebApplicationFactory();
        int categoryId;
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var category = new InventoryPlatform.Domain.Entities.Category("Test Category", "Test Description");
            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();
            categoryId = category.Id;
        }

        var cookieContainer = new CookieContainer();
        using var client = CreateClient(factory, cookieContainer, TestUserSelectors.InventoryManager);

        // GET the Edit page
        var editPath = $"{CategoryEditPathPrefix}{categoryId}";
        var getResponse = await client.GetAsync(editPath);
        var getHtml = await getResponse.Content.ReadAsStringAsync();
        Assert.True(getResponse.StatusCode == HttpStatusCode.OK);

        // Extract form action and antiforgery token
        var formAction = CategoryEditFormExtraction.ExtractEditFormAction(getHtml, categoryId);
        var antiforgeryToken = CategoryEditFormExtraction.ExtractEditFormAntiforgeryToken(getHtml);

        // POST with empty name (validation failure)
        var emptyName = "";
        var description = "Updated description";

        using var postRequest = new HttpRequestMessage(HttpMethod.Post, formAction)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [AntiforgeryTokenFieldName] = antiforgeryToken,
                ["Category.Id"] = categoryId.ToString(),
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

        // Assert - input is preserved
        Assert.Contains($"name=\"Category.Id\"", postHtml, StringComparison.Ordinal);
        Assert.Contains($"value=\"{categoryId}\"", postHtml, StringComparison.Ordinal);
        Assert.Contains("name=\"Category.Name\"", postHtml, StringComparison.Ordinal);
        Assert.Contains("name=\"Category.Description\"", postHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PostCategoryEdit_ValidationFailureNameTooLong_DisplaysErrorAndPreservesInput()
    {
        // Arrange - create a category first
        await using var factory = new InventoryPlatformWebApplicationFactory();
        int categoryId;
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var category = new InventoryPlatform.Domain.Entities.Category("Test Category", "Test Description");
            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();
            categoryId = category.Id;
        }

        var cookieContainer = new CookieContainer();
        using var client = CreateClient(factory, cookieContainer, TestUserSelectors.InventoryManager);

        var editPath = $"{CategoryEditPathPrefix}{categoryId}";
        var getResponse = await client.GetAsync(editPath);
        var getHtml = await getResponse.Content.ReadAsStringAsync();
        Assert.True(getResponse.StatusCode == HttpStatusCode.OK);

        var formAction = CategoryEditFormExtraction.ExtractEditFormAction(getHtml, categoryId);
        var antiforgeryToken = CategoryEditFormExtraction.ExtractEditFormAntiforgeryToken(getHtml);

        // POST with name too long (101 characters)
        var longName = new string('x', 101);
        var description = "Updated description";

        using var postRequest = new HttpRequestMessage(HttpMethod.Post, formAction)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [AntiforgeryTokenFieldName] = antiforgeryToken,
                ["Category.Id"] = categoryId.ToString(),
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
        Assert.Contains($"name=\"Category.Id\"", postHtml, StringComparison.Ordinal);
        Assert.Contains("name=\"Category.Name\"", postHtml, StringComparison.Ordinal);
        Assert.Contains("name=\"Category.Description\"", postHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PostCategoryEdit_ValidationFailureInvalidId_DisplaysError()
    {
        // Arrange - create a category first
        await using var factory = new InventoryPlatformWebApplicationFactory();
        int categoryId;
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var category = new InventoryPlatform.Domain.Entities.Category("Test Category", "Test Description");
            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();
            categoryId = category.Id;
        }

        var cookieContainer = new CookieContainer();
        using var client = CreateClient(factory, cookieContainer, TestUserSelectors.InventoryManager);

        var editPath = $"{CategoryEditPathPrefix}{categoryId}";
        var getResponse = await client.GetAsync(editPath);
        var getHtml = await getResponse.Content.ReadAsStringAsync();
        Assert.True(getResponse.StatusCode == HttpStatusCode.OK);

        var formAction = CategoryEditFormExtraction.ExtractEditFormAction(getHtml, categoryId);
        var antiforgeryToken = CategoryEditFormExtraction.ExtractEditFormAntiforgeryToken(getHtml);

        // POST with invalid Id (0)
        var invalidId = 0;
        var name = "Valid Name";
        var description = "Updated description";

        using var postRequest = new HttpRequestMessage(HttpMethod.Post, formAction)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [AntiforgeryTokenFieldName] = antiforgeryToken,
                ["Category.Id"] = invalidId.ToString(),
                ["Category.Name"] = name,
                ["Category.Description"] = description
            })
        };

        var postResponse = await client.SendAsync(postRequest);
        var postHtml = await postResponse.Content.ReadAsStringAsync();

        // Assert - validation failure returns 200 (Page())
        Assert.True(postResponse.StatusCode == HttpStatusCode.OK,
            $"Expected HTTP 200 on validation failure but received {(int)postResponse.StatusCode}.{Environment.NewLine}{postHtml}");

        // Assert - validation summary contains the error message (Id must be > 0)
        var validationSummary = ExtractValidationSummaryText(postHtml);
        Assert.Contains("greater than", validationSummary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task PostCategoryEdit_ValidRequest_RedirectsAndUpdates()
    {
        // Arrange - create a category first
        await using var factory = new InventoryPlatformWebApplicationFactory();
        int categoryId;
        
        using (var scope = factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var category = new InventoryPlatform.Domain.Entities.Category("Test Category", "Test Description");
            dbContext.Categories.Add(category);
            await dbContext.SaveChangesAsync();
            categoryId = category.Id;
        }

        var cookieContainer = new CookieContainer();
        using var client = CreateClient(factory, cookieContainer, TestUserSelectors.InventoryManager);

        var editPath = $"{CategoryEditPathPrefix}{categoryId}";
        var getResponse = await client.GetAsync(editPath);
        var getHtml = await getResponse.Content.ReadAsStringAsync();
        Assert.True(getResponse.StatusCode == HttpStatusCode.OK);

        var formAction = CategoryEditFormExtraction.ExtractEditFormAction(getHtml, categoryId);
        var antiforgeryToken = CategoryEditFormExtraction.ExtractEditFormAntiforgeryToken(getHtml);

        // POST with valid data
        var updatedName = $"Updated Category {Guid.NewGuid():N}";
        var updatedDescription = $"Updated description {Guid.NewGuid():N}";

        using var postRequest = new HttpRequestMessage(HttpMethod.Post, formAction)
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                [AntiforgeryTokenFieldName] = antiforgeryToken,
                ["Category.Id"] = categoryId.ToString(),
                ["Category.Name"] = updatedName,
                ["Category.Description"] = updatedDescription
            })
        };

        var postResponse = await client.SendAsync(postRequest);

        // Assert - successful redirect (PRG)
        Assert.True(postResponse.StatusCode == HttpStatusCode.Redirect,
            $"Expected HTTP 302 (Post-Redirect-Get) but received {(int)postResponse.StatusCode}");

        var location = Assert.IsType<Uri>(postResponse.Headers.Location);
        var redirectDestination = location.IsAbsoluteUri ? location : new Uri(client.BaseAddress!, location);
        Assert.Equal(CategoryIndexPath, redirectDestination.AbsolutePath);

        // Assert - persistence verified
        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var persistedCategory = await verifyDb.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == categoryId);
        
        Assert.NotNull(persistedCategory);
        Assert.Equal(updatedName, persistedCategory!.Name);
        Assert.Equal(updatedDescription, persistedCategory.Description);
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

    private static string ExtractValidationSummaryText(string html)
    {
        var summary = Regex.Match(
            html,
            @"<div\b(?=[^>]*\bclass=""[^""]*\bvalidation-summary-errors\b[^""]*"")[^>]*>(?<content>.*?)</div>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        if (!summary.Success)
        {
            throw new InvalidOperationException(
                "The Category Edit page did not contain a model-level " +
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

internal static class CategoryEditFormExtraction
{
    private const string AntiforgeryTokenFieldName = "__RequestVerificationToken";

    private static readonly Regex FormOpenTagRegex = new(
        @"<form\b(?<attributes>[^>]*)>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex InputTagRegex = new(
        @"<input\b(?<attributes>[^>]*?)/?>",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static string ExtractEditFormAction(string html, int categoryId)
    {
        foreach (Match formOpenTag in FormOpenTagRegex.Matches(html))
        {
            var attributes = formOpenTag.Groups["attributes"].Value;
            var action = GetAttributeValue(attributes, "action");

            // The Edit form posts to the current URL with the id in route
            var expectedPath = $"/Categories/Edit/{categoryId}";
            if (action is null && HasMethodPostAttribute(attributes))
            {
                return expectedPath;
            }

            if (action is not null && action.Contains(expectedPath, StringComparison.Ordinal))
            {
                return action;
            }
        }

        throw new InvalidOperationException(
            $"The response did not contain a rendered form posting to /Categories/Edit/{categoryId}.");
    }

    public static string ExtractEditFormAntiforgeryToken(string html)
    {
        foreach (Match formOpenTag in FormOpenTagRegex.Matches(html))
        {
            var attributes = formOpenTag.Groups["attributes"].Value;
            var action = GetAttributeValue(attributes, "action");
            var expectedPath = "/Categories/Edit/";

            if ((action is null || action.Contains(expectedPath, StringComparison.Ordinal))
                && HasMethodPostAttribute(attributes))
            {
                var formCloseIndex = html.IndexOf("</form>", formOpenTag.Index + formOpenTag.Length, StringComparison.OrdinalIgnoreCase);
                if (formCloseIndex < 0)
                {
                    continue;
                }

                var formRegion = html.Substring(formOpenTag.Index, formCloseIndex - formOpenTag.Index);

                foreach (Match inputTag in InputTagRegex.Matches(formRegion))
                {
                    var inputAttributes = inputTag.Groups["attributes"].Value;
                    var name = GetAttributeValue(inputAttributes, "name");

                    if (string.Equals(name, AntiforgeryTokenFieldName, StringComparison.Ordinal))
                    {
                        var value = GetAttributeValue(inputAttributes, "value");
                        return string.IsNullOrWhiteSpace(value)
                            ? throw new InvalidOperationException("The rendered antiforgery hidden input did not contain a token value.")
                            : value;
                    }
                }
            }
        }

        throw new InvalidOperationException("The Category Edit form did not contain a hidden antiforgery token.");
    }

    private static bool HasMethodPostAttribute(string attributes)
    {
        return Regex.IsMatch(attributes, "\\bmethod\\s*=\\s*\"post\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static string? GetAttributeValue(string attributes, string attributeName)
    {
        var match = Regex.Match(attributes, $"\\b{Regex.Escape(attributeName)}\\s*=\\s*\"(?<value>[^\"]*)\"", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        return match.Success ? WebUtility.HtmlDecode(match.Groups["value"].Value) : null;
    }
}