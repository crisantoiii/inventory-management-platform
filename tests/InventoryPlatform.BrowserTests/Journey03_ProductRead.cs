using InventoryPlatform.BrowserTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;

namespace InventoryPlatform.BrowserTests;

public sealed class Journey03_ProductRead
{
    [Fact]
    public async Task Manager_can_read_seeded_product_on_products_page()
    {
        await using var run = await BrowserTestContext.StartAsync();
        await using var browserContext = await run.CreateJourneyBrowserContextAsync();
        var page = await browserContext.NewPageAsync();

        try
        {
            var stateBefore = await ReadSeededProductStateAsync(run);

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

            await page.GotoAsync(
                $"{run.BaseUrl}/Products",
                new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle
                });

            Assert.Equal("/Products", new Uri(page.Url).AbsolutePath);

            var productRow = page.Locator(
                "table.table-striped tbody tr",
                new PageLocatorOptions
                {
                    HasTextString = BrowserSeedData.ProductName
                });

            Assert.Equal(1, await productRow.CountAsync());

            var productNameText = (await productRow
                .Locator("td:nth-child(2) a")
                .TextContentAsync())?.Trim();
            Assert.Equal(BrowserSeedData.ProductName, productNameText);

            var skuText = (await productRow
                .Locator("td:nth-child(1)")
                .TextContentAsync())?.Trim();
            Assert.Equal(BrowserSeedData.ProductSku, skuText);

            var stateAfter = await ReadSeededProductStateAsync(run);
            Assert.Equal(stateBefore, stateAfter);
        }
        catch
        {
            await run.CaptureFailureArtifactsAsync("Journey03_ProductRead", page);
            throw;
        }
    }

    private static async Task<SeededProductState> ReadSeededProductStateAsync(
        BrowserTestContext context)
    {
        await using var db = context.CreateDbContext();

        var productCount = await db.Products.CountAsync();
        var product = await db.Products
            .SingleAsync(item => item.Sku == BrowserSeedData.ProductSku);

        return new SeededProductState(
            productCount,
            product.Id,
            product.Name,
            product.Sku,
            product.CategoryId,
            product.UnitId,
            product.QuantityOnHand,
            product.CostPrice,
            product.SellingPrice,
            product.IsActive);
    }

    private sealed record SeededProductState(
        int ProductCount,
        int Id,
        string Name,
        string Sku,
        int CategoryId,
        int UnitId,
        decimal QuantityOnHand,
        decimal CostPrice,
        decimal SellingPrice,
        bool IsActive);
}
