using System.Net;
using InventoryPlatform.BrowserTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;

namespace InventoryPlatform.BrowserTests;

public sealed class Journey02_AuthorizationDenial
{
    [Fact]
    public async Task Viewer_is_denied_category_create_and_no_category_is_created()
    {
        await using var run = await BrowserTestContext.StartAsync();
        await using var browserContext = await run.CreateJourneyBrowserContextAsync();
        var page = await browserContext.NewPageAsync();

        try
        {
            var beforeCategoryCount = await CountCategoriesAsync(run);

            await page.GotoAsync(
                $"{run.BaseUrl}/Identity/Account/Login",
                new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle
                });

            var emailField = page.Locator("input[name='Input.Email']");
            var passwordField = page.Locator("input[name='Input.Password']");

            Assert.True(await emailField.IsVisibleAsync());
            Assert.True(await passwordField.IsVisibleAsync());

            await emailField.FillAsync("viewer@inventory.local");
            await passwordField.FillAsync("Viewer@123");

            await page.Locator("button[type='submit']").ClickAsync();

            await page.WaitForURLAsync(
                url =>
                    url.StartsWith(run.BaseUrl, StringComparison.Ordinal) &&
                    url.Contains("/Dashboard", StringComparison.Ordinal),
                new PageWaitForURLOptions
                {
                    Timeout = 15000
                });

            await page.GotoAsync(
                $"{run.BaseUrl}/Categories/Create",
                new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle
                });

            await page.WaitForURLAsync(
                url => url.Contains("/Identity/Account/AccessDenied", StringComparison.Ordinal),
                new PageWaitForURLOptions
                {
                    Timeout = 15000
                });

            var finalUri = new Uri(page.Url);
            Assert.Equal("/Identity/Account/AccessDenied", finalUri.AbsolutePath);

            var decodedQuery = WebUtility.UrlDecode(finalUri.Query);
            if (decodedQuery.Contains("ReturnUrl", StringComparison.Ordinal))
            {
                Assert.Contains("/Categories/Create", decodedQuery, StringComparison.Ordinal);
            }

            var accessDeniedHeading = await page.Locator("h2").TextContentAsync();
            Assert.Contains("Access Denied", accessDeniedHeading ?? string.Empty, StringComparison.Ordinal);

            Assert.Equal(0, await page.Locator("input[name='Category.Name']").CountAsync());

            var afterCategoryCount = await CountCategoriesAsync(run);
            Assert.Equal(beforeCategoryCount, afterCategoryCount);
        }
        catch
        {
            await run.CaptureFailureArtifactsAsync("Journey02_AuthorizationDenial", page);
            throw;
        }
    }

    private static async Task<int> CountCategoriesAsync(BrowserTestContext context)
    {
        await using var db = context.CreateDbContext();
        return await db.Categories.CountAsync();
    }
}
