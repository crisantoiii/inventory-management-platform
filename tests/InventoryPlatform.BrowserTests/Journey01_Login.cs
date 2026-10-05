using System.Net;
using InventoryPlatform.BrowserTests.Infrastructure;
using Microsoft.Playwright;

namespace InventoryPlatform.BrowserTests;

public sealed class Journey01_Login
{
    [Fact]
    public async Task Manager_can_sign_in_and_reach_homepage()
    {
        await using var run = await BrowserTestContext.StartAsync();
        await using var browserContext = await run.CreateJourneyBrowserContextAsync();
        var page = await browserContext.NewPageAsync();

        try
        {
            await page.GotoAsync(
                $"{run.BaseUrl}/Identity/Account/Login?ReturnUrl=%2F",
                new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle
                });

            Assert.Contains("/Identity/Account/Login", page.Url);

            var emailField = page.Locator("input[name='Input.Email']");
            var passwordField = page.Locator("input[name='Input.Password']");

            Assert.True(await emailField.IsVisibleAsync());
            Assert.True(await passwordField.IsVisibleAsync());

            await emailField.FillAsync("manager@inventory.local");
            await passwordField.FillAsync("Manager@123");

            await page.Locator("button[type='submit']").ClickAsync();

            await page.WaitForURLAsync(
                url =>
                    url.StartsWith(run.BaseUrl, StringComparison.Ordinal) &&
                    url.Contains("/Dashboard", StringComparison.Ordinal),
                new PageWaitForURLOptions
                {
                    Timeout = 15000
                });

            var finalUri = new Uri(page.Url);
            Assert.Equal("/Dashboard", finalUri.AbsolutePath);

            var headingText = await page.Locator("h1").TextContentAsync();
            Assert.Contains(
                "Dashboard",
                headingText ?? string.Empty,
                StringComparison.Ordinal);
        }
        catch
        {
            await run.CaptureFailureArtifactsAsync("Journey01_Login", page);
            throw;
        }
    }
}
