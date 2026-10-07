using System.Globalization;
using InventoryPlatform.BrowserTests.Infrastructure;
using InventoryPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;

namespace InventoryPlatform.BrowserTests;

public sealed class Journey04_PurchaseOrderCreateSubmit
{
    private const string ExpectedDeliveryDate = "2030-01-15";
    private const string ItemQuantity = "3";
    private const string ItemUnitCost = "12.50";

    [Fact]
    public async Task Manager_can_create_and_submit_purchase_order()
    {
        await using var run = await BrowserTestContext.StartAsync();
        await using var browserContext = await run.CreateJourneyBrowserContextAsync();
        var page = await browserContext.NewPageAsync();

        try
        {
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

            var supplierId = await ResolveSeededSupplierIdAsync(run);
            var productId = await ResolveSeededProductIdAsync(run);

            var remarksMarker = $"E2E Smoke PO {run.RunId}";

            await page.GotoAsync(
                $"{run.BaseUrl}/Purchasing/PurchaseOrders/Create",
                new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle
                });

            Assert.Equal(
                "/Purchasing/PurchaseOrders/Create",
                new Uri(page.Url).AbsolutePath);

            await page
                .Locator("select[name='PurchaseOrder.SupplierId']")
                .SelectOptionAsync(
                    supplierId.ToString(CultureInfo.InvariantCulture));

            await page
                .Locator("input[name='PurchaseOrder.ExpectedDeliveryDate']")
                .FillAsync(ExpectedDeliveryDate);

            await page
                .Locator("textarea[name='PurchaseOrder.Remarks']")
                .FillAsync(remarksMarker);

            await page
                .Locator("select[name='PurchaseOrder.Items[0].ProductId']")
                .SelectOptionAsync(
                    productId.ToString(CultureInfo.InvariantCulture));

            await page
                .Locator("input[name='PurchaseOrder.Items[0].Quantity']")
                .FillAsync(ItemQuantity);

            await page
                .Locator("input[name='PurchaseOrder.Items[0].UnitCost']")
                .FillAsync(ItemUnitCost);

            await page
                .GetByRole(
                    AriaRole.Button,
                    new PageGetByRoleOptions
                    {
                        Name = "Create Purchase Order",
                        Exact = true
                    })
                .ClickAsync();

            await page.WaitForURLAsync(
                url =>
                    url.StartsWith(run.BaseUrl, StringComparison.Ordinal) &&
                    url.Contains("/Purchasing/PurchaseOrders", StringComparison.Ordinal) &&
                    !url.Contains("/Create", StringComparison.Ordinal) &&
                    !url.Contains("handler=", StringComparison.Ordinal),
                new PageWaitForURLOptions
                {
                    Timeout = 15000
                });

            var createdOrder = await FindCreatedPurchaseOrderAsync(
                run,
                remarksMarker,
                supplierId,
                productId);

            Assert.Equal(PurchaseOrderStatus.Draft, createdOrder.Status);
            Assert.Equal(
                new DateOnly(2030, 1, 15),
                createdOrder.ExpectedDeliveryDate);

            await page.GotoAsync(
                $"{run.BaseUrl}/Purchasing/PurchaseOrders/Details/{createdOrder.Id}",
                new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle
                });

            Assert.Equal(
                $"/Purchasing/PurchaseOrders/Details/{createdOrder.Id}",
                new Uri(page.Url).AbsolutePath);

            Assert.Equal("Draft", await ReadStatusTextAsync(page));

            await page
                .GetByRole(
                    AriaRole.Button,
                    new PageGetByRoleOptions
                    {
                        Name = "Submit Purchase Order",
                        Exact = true
                    })
                .ClickAsync();

            await page.WaitForURLAsync(
                url =>
                    url.StartsWith(run.BaseUrl, StringComparison.Ordinal) &&
                    url.Contains(
                        $"/Purchasing/PurchaseOrders/Details/{createdOrder.Id}",
                        StringComparison.Ordinal) &&
                    !url.Contains("handler=", StringComparison.Ordinal),
                new PageWaitForURLOptions
                {
                    Timeout = 15000
                });

            Assert.Equal(
                $"/Purchasing/PurchaseOrders/Details/{createdOrder.Id}",
                new Uri(page.Url).AbsolutePath);

            Assert.Equal("Submitted", await ReadStatusTextAsync(page));

            var persistedStatus = await ReadPersistedStatusAsync(
                run,
                createdOrder.Id);

            Assert.Equal(PurchaseOrderStatus.Submitted, persistedStatus);
        }
        catch
        {
            await run.CaptureFailureArtifactsAsync(
                "Journey04_PurchaseOrderCreateSubmit",
                page);
            throw;
        }
    }

    private static async Task<int> ResolveSeededSupplierIdAsync(
        BrowserTestContext context)
    {
        await using var db = context.CreateDbContext();

        return await db.Suppliers
            .Where(supplier => supplier.Name == BrowserSeedData.SupplierName)
            .Select(supplier => supplier.Id)
            .SingleAsync();
    }

    private static async Task<int> ResolveSeededProductIdAsync(
        BrowserTestContext context)
    {
        await using var db = context.CreateDbContext();

        return await db.Products
            .Where(product => product.Sku == BrowserSeedData.ProductSku)
            .Select(product => product.Id)
            .SingleAsync();
    }

    private static async Task<CreatedPurchaseOrder> FindCreatedPurchaseOrderAsync(
        BrowserTestContext context,
        string remarksMarker,
        int supplierId,
        int productId)
    {
        await using var db = context.CreateDbContext();

        var matches = await db.PurchaseOrders
            .Include(order => order.Items)
            .Where(order =>
                order.Remarks == remarksMarker &&
                order.SupplierId == supplierId)
            .ToListAsync();

        var order = Assert.Single(matches);
        var item = Assert.Single(order.Items);

        Assert.Equal(productId, item.ProductId);
        Assert.Equal(3m, item.Quantity);
        Assert.Equal(12.50m, item.UnitCost);

        return new CreatedPurchaseOrder(
            order.Id,
            order.SupplierId,
            order.Remarks,
            order.ExpectedDeliveryDate,
            order.Status,
            item.ProductId,
            item.Quantity,
            item.UnitCost);
    }

    private static async Task<PurchaseOrderStatus> ReadPersistedStatusAsync(
        BrowserTestContext context,
        int purchaseOrderId)
    {
        await using var db = context.CreateDbContext();

        return await db.PurchaseOrders
            .Where(order => order.Id == purchaseOrderId)
            .Select(order => order.Status)
            .SingleAsync();
    }

    private static async Task<string?> ReadStatusTextAsync(IPage page)
    {
        var statusBadge = page.Locator(
            "label:text-is('Status') + div span.badge.bg-secondary");

        Assert.Equal(1, await statusBadge.CountAsync());

        return (await statusBadge.TextContentAsync())?.Trim();
    }

    private sealed record CreatedPurchaseOrder(
        int Id,
        int SupplierId,
        string? Remarks,
        DateOnly? ExpectedDeliveryDate,
        PurchaseOrderStatus Status,
        int ProductId,
        decimal Quantity,
        decimal UnitCost);
}
