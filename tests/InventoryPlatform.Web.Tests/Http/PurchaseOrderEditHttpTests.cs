using System.Globalization;
using System.Net;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
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

/// <summary>
/// Sprint 22 Purchase Order Edit HTTP foundation.
///
/// T01 intentionally contains no behavioral xUnit test. T02 and T03 will add
/// exactly the frozen H1-H6 cases while reusing these fixture-local helpers.
/// </summary>
public sealed class PurchaseOrderEditHttpTests
{
    private const string EditPathPrefix =
        "/Purchasing/PurchaseOrders/Edit/";

    private const string NavigationSearch = "S22-Edit-Nav";
    private const string NavigationFromDate = "2026-01-01";
    private const string NavigationToDate = "2026-12-31";
    private const string NavigationStatus = "Draft";
    private const string NavigationSortBy = "OrderDate";
    private const int NavigationPageNum = 2;
    private const int NavigationPageSize = 25;

    private static async Task<PurchaseOrderEditArrangement>
        ArrangeOneItemPurchaseOrderAsync(
            InventoryPlatformWebApplicationFactory factory,
            bool submit = false)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var marker = Guid.NewGuid().ToString("N");
        var supplier = new Supplier(
            $"S22 Edit Supplier {marker}",
            contactPerson: null,
            email: null,
            phone: null,
            address: null);
        var category = new Category(
            $"S22 Edit Category {marker}",
            description: null);
        var unit = new Unit(
            $"S22{marker[..8]}",
            $"S22 Edit Unit {marker}",
            "ea");

        context.AddRange(supplier, category, unit);
        await context.SaveChangesAsync();

        var product = new Product(
            $"S22-{marker[..12]}",
            $"S22 Edit Product {marker}",
            category.Id,
            unit.Id,
            quantityOnHand: 7m,
            costPrice: 12.50m,
            sellingPrice: 20.00m);

        context.Products.Add(product);
        await context.SaveChangesAsync();

        var purchaseOrder = PurchaseOrder.Create(
            supplier.Id,
            new DateOnly(2026, 9, 1),
            expectedDeliveryDate: new DateOnly(2026, 10, 1),
            remarks: $"S22 Edit Draft {marker}");

        purchaseOrder.AddItem(
            product.Id,
            quantity: 5m,
            unitCost: 12.50m);

        if (submit)
        {
            purchaseOrder.Submit();
        }

        context.PurchaseOrders.Add(purchaseOrder);
        await context.SaveChangesAsync();

        return new PurchaseOrderEditArrangement(
            purchaseOrder.Id,
            product.Id,
            supplier.Name,
            product.Sku,
            product.Name,
            CaptureState(purchaseOrder));
    }

    private static async Task<Guid> ArrangeDeniedUserAsync(
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

    private static async Task<PurchaseOrderPersistedState> ReadStateAsync(
        InventoryPlatformWebApplicationFactory factory,
        int purchaseOrderId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        var purchaseOrder = await context.PurchaseOrders
            .AsNoTracking()
            .Include(order => order.Items)
            .SingleAsync(order => order.Id == purchaseOrderId);

        return CaptureState(purchaseOrder);
    }

    private static PurchaseOrderPersistedState CaptureState(
        PurchaseOrder purchaseOrder) =>
        new(
            purchaseOrder.SupplierId,
            purchaseOrder.OrderDate,
            purchaseOrder.ExpectedDeliveryDate,
            purchaseOrder.Remarks,
            purchaseOrder.Status,
            purchaseOrder.TotalAmount,
            purchaseOrder.Items
                .OrderBy(item => item.ProductId)
                .Select(item => new PurchaseOrderItemPersistedState(
                    item.ProductId,
                    item.Quantity,
                    item.UnitCost,
                    item.ReceivedQuantity))
                .ToArray());

    private static string BuildEditPathWithNavigation(int purchaseOrderId) =>
        $"{EditPathPrefix}{purchaseOrderId}" +
        $"?Search={Uri.EscapeDataString(NavigationSearch)}" +
        $"&FromDate={NavigationFromDate}" +
        $"&ToDate={NavigationToDate}" +
        $"&Status={NavigationStatus}" +
        $"&SortBy={NavigationSortBy}" +
        "&Descending=true" +
        $"&PageNum={NavigationPageNum}" +
        $"&PageSize={NavigationPageSize}";

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

    private static HttpClient CreateCookiePreservingManagerClient(
        InventoryPlatformWebApplicationFactory factory,
        CookieContainer cookieContainer)
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

        client.DefaultRequestHeaders.Add(
            TestAuthenticationDefaults.UserHeader,
            TestUserSelectors.InventoryManager);

        return client;
    }

    private static void AssertEditNavigation(
        Uri location,
        int purchaseOrderId)
    {
        var destination = location.IsAbsoluteUri
            ? location
            : new Uri(new Uri("https://localhost"), location);
        var query = QueryHelpers.ParseQuery(destination.Query);
        var fromDate = Assert.Single(query["FromDate"]);
        var toDate = Assert.Single(query["ToDate"]);

        Assert.NotNull(fromDate);
        Assert.NotNull(toDate);

        Assert.Equal($"{EditPathPrefix}{purchaseOrderId}", destination.AbsolutePath);
        Assert.Equal(NavigationSearch, Assert.Single(query["Search"]));
        Assert.Equal(
            new DateOnly(2026, 1, 1),
            DateOnly.Parse(fromDate));
        Assert.Equal(
            new DateOnly(2026, 12, 31),
            DateOnly.Parse(toDate));
        Assert.Equal(NavigationStatus, Assert.Single(query["Status"]));
        Assert.Equal(NavigationSortBy, Assert.Single(query["SortBy"]));
        Assert.Equal("True", Assert.Single(query["Descending"]), ignoreCase: true);
        Assert.Equal(
            NavigationPageNum.ToString(CultureInfo.InvariantCulture),
            Assert.Single(query["PageNum"]));
        Assert.Equal(
            NavigationPageSize.ToString(CultureInfo.InvariantCulture),
            Assert.Single(query["PageSize"]));
    }

    private sealed record PurchaseOrderEditArrangement(
        int PurchaseOrderId,
        int ProductId,
        string SupplierName,
        string ProductSku,
        string ProductName,
        PurchaseOrderPersistedState InitialState);

    private sealed record PurchaseOrderPersistedState(
        int SupplierId,
        DateOnly OrderDate,
        DateOnly? ExpectedDeliveryDate,
        string? Remarks,
        PurchaseOrderStatus Status,
        decimal TotalAmount,
        IReadOnlyList<PurchaseOrderItemPersistedState> Items);

    private sealed record PurchaseOrderItemPersistedState(
        int ProductId,
        decimal Quantity,
        decimal UnitCost,
        decimal ReceivedQuantity);

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
                request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
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
