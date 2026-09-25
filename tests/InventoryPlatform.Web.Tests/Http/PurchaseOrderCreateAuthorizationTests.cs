using System.Net;
using System.Text.RegularExpressions;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Infrastructure.Identity;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Web.Tests.Authentication;
using InventoryPlatform.Web.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InventoryPlatform.Web.Tests.Http;

public sealed class PurchaseOrderCreateAuthorizationTests
{
    private const string PurchaseOrderCreatePath =
        "/Purchasing/PurchaseOrders/Create";

    [Fact]
    public async Task GetPurchaseOrderCreate_WithoutTestUser_RedirectsToIdentityLogin()
    {
        await using var factory = new InventoryPlatformWebApplicationFactory();
        using var client = CreateClient(factory);

        var response = await client.GetAsync(PurchaseOrderCreatePath);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        AssertRedirect(
            client,
            response,
            expectedPath: "/Identity/Account/Login",
            expectedReturnUrl: PurchaseOrderCreatePath);
        Assert.False(client.DefaultRequestHeaders.Contains(
            TestAuthenticationDefaults.UserHeader));
    }

    [Fact]
    public async Task GetPurchaseOrderCreate_AsInventoryManager_RendersFormWithArrangedOptions()
    {
        await using var factory = new InventoryPlatformWebApplicationFactory();
        var arrangedData = await ArrangePurchaseOrderOptionsAsync(factory);
        using var client = CreateClient(factory, TestUserSelectors.InventoryManager);

        var response = await client.GetAsync(PurchaseOrderCreatePath);
        var content = await response.Content.ReadAsStringAsync();

        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected HTTP 200 but received {(int)response.StatusCode}." +
            $"{Environment.NewLine}{content}");
        Assert.Contains("Create Purchase Order", content, StringComparison.Ordinal);
        Assert.Contains("name=\"PurchaseOrder.SupplierId\"", content, StringComparison.Ordinal);
        Assert.Contains("name=\"PurchaseOrder.ExpectedDeliveryDate\"", content, StringComparison.Ordinal);
        Assert.Contains("name=\"PurchaseOrder.Remarks\"", content, StringComparison.Ordinal);
        Assert.Contains("name=\"PurchaseOrder.Items[0].ProductId\"", content, StringComparison.Ordinal);
        Assert.Contains("name=\"PurchaseOrder.Items[0].Quantity\"", content, StringComparison.Ordinal);
        Assert.Contains("name=\"PurchaseOrder.Items[0].UnitCost\"", content, StringComparison.Ordinal);
        Assert.Contains("id=\"purchase-order-items\"", content, StringComparison.Ordinal);
        Assert.Contains("data-item-row", content, StringComparison.Ordinal);
        AssertSelectContainsOption(
            content,
            "PurchaseOrder.SupplierId",
            arrangedData.SupplierId,
            "S19 Purchase Order Supplier");
        AssertSelectContainsOption(
            content,
            "PurchaseOrder.Items[0].ProductId",
            arrangedData.ProductId,
            "S19 Purchase Order Product");
    }

    [Fact]
    public async Task GetPurchaseOrderCreate_AsUserWithoutCapability_RedirectsToIdentityAccessDenied()
    {
        await using var factory = new InventoryPlatformWebApplicationFactory();
        var userId = await ArrangeUserWithoutAuthorizationGroupAsync(factory);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Assert.False(await context.UserAuthorizationGroups
                .AnyAsync(relationship => relationship.UserId == userId));
        }

        using var client = CreateClient(factory, TestUserSelectors.PurchaseOrderDenied);

        var response = await client.GetAsync(PurchaseOrderCreatePath);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        AssertRedirect(
            client,
            response,
            expectedPath: "/Identity/Account/AccessDenied",
            expectedReturnUrl: PurchaseOrderCreatePath);
        Assert.Equal(
            TestUserSelectors.PurchaseOrderDenied,
            client.DefaultRequestHeaders.GetValues(
                TestAuthenticationDefaults.UserHeader).Single());
    }

    private static async Task<(int SupplierId, int ProductId)>
        ArrangePurchaseOrderOptionsAsync(
            InventoryPlatformWebApplicationFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var supplier = new Supplier(
            "S19 Purchase Order Supplier",
            contactPerson: null,
            email: null,
            phone: null,
            address: null);
        var category = new Category(
            "S19 Purchase Order Category",
            description: null);
        var unit = new Unit(
            "S19UNIT",
            "S19 Purchase Order Unit",
            "s19");

        context.AddRange(supplier, category, unit);
        await context.SaveChangesAsync();

        var product = new Product(
            "S19-PO-PRODUCT",
            "S19 Purchase Order Product",
            category.Id,
            unit.Id,
            quantityOnHand: 0m,
            costPrice: 12.50m,
            sellingPrice: 20.00m);

        context.Products.Add(product);
        await context.SaveChangesAsync();

        return (supplier.Id, product.Id);
    }

    private static async Task<Guid> ArrangeUserWithoutAuthorizationGroupAsync(
        InventoryPlatformWebApplicationFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "purchaseorder-denied",
            Email = TestUserSelectors.PurchaseOrderDenied,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user);

        Assert.True(
            result.Succeeded,
            "Creating the test-only Purchase Order denied user failed: " +
            string.Join(
                Environment.NewLine,
                result.Errors.Select(error => error.Description)));

        return user.Id;
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

    private static void AssertSelectContainsOption(
        string html,
        string selectName,
        int optionValue,
        string optionText)
    {
        var select = Regex.Match(
            html,
            $"<select\\b(?=[^>]*\\bname=\\\"{Regex.Escape(selectName)}\\\")[^>]*>(?<content>.*?)</select>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        Assert.True(select.Success, $"Select '{selectName}' was not rendered.");

        var option = Regex.Match(
            select.Groups["content"].Value,
            $"<option\\b(?=[^>]*\\bvalue=\\\"{optionValue}\\\")[^>]*>\\s*{Regex.Escape(optionText)}\\s*</option>",
            RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        Assert.True(
            option.Success,
            $"Select '{selectName}' did not contain option '{optionValue}' / '{optionText}'.");
    }
}
