using System.Globalization;
using System.Net;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Infrastructure.Identity;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Web.Authorization;
using InventoryPlatform.Web.Tests.Authentication;
using InventoryPlatform.Web.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InventoryPlatform.Web.Tests.Http;

/// <summary>Sprint 23 lifecycle HTTP support; behavioral tests are deferred.</summary>
public sealed class PurchaseOrderLifecycleHttpTests
{
    private const string DetailsPrefix = "/Purchasing/PurchaseOrders/Details/";
    private static readonly LifecycleNavigation Navigation = new(
        "S23 lifecycle / retained", new DateOnly(2026, 1, 1),
        new DateOnly(2026, 12, 31), "Submitted", "OrderDate", true, 3, 25);

    [Fact]
    public async Task PostApprove_AsInventoryManager_ApprovesAndPreservesNavigation()
    {
        using var factory = new InventoryPlatformWebApplicationFactory();
        var arrangement = await ArrangeSubmittedAsync(factory);
        using var client = CreateClient(
            factory, TestUserSelectors.InventoryManager, new CookieContainer());

        var details = await client.GetAsync(BuildDetailsPath(arrangement.PurchaseOrderId));
        Assert.Equal(HttpStatusCode.OK, details.StatusCode);
        var form = PurchaseOrderLifecycleFormExtraction.ExtractApproveForm(
            await details.Content.ReadAsStringAsync());
        AssertFormNavigation(form);

        var response = await client.PostAsync(form.Action, CreateFormContent(form));

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        AssertDetailsDestination(response.Headers.Location, arrangement.PurchaseOrderId);
        var actual = await ReadStateAsync(factory, arrangement);
        AssertOnlyStatusChanged(
            arrangement.InitialState, actual, PurchaseOrderStatus.Approved);
    }

    [Fact]
    public async Task PostCancel_AsInventoryManager_CancelsAndPreservesNavigation()
    {
        using var factory = new InventoryPlatformWebApplicationFactory();
        var arrangement = await ArrangeSubmittedAsync(factory);
        using var client = CreateClient(
            factory, TestUserSelectors.InventoryManager, new CookieContainer());

        var details = await client.GetAsync(BuildDetailsPath(arrangement.PurchaseOrderId));
        Assert.Equal(HttpStatusCode.OK, details.StatusCode);
        var form = PurchaseOrderLifecycleFormExtraction.ExtractCancelForm(
            await details.Content.ReadAsStringAsync());
        AssertFormNavigation(form);

        var response = await client.PostAsync(form.Action, CreateFormContent(form));

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        AssertDetailsDestination(response.Headers.Location, arrangement.PurchaseOrderId);
        var actual = await ReadStateAsync(factory, arrangement);
        AssertOnlyStatusChanged(
            arrangement.InitialState, actual, PurchaseOrderStatus.Cancelled);
    }

    [Fact]
    public async Task PostStaleCancel_AfterApproval_RedisplaysForbiddenStateWithoutMutation()
    {
        using var factory = new InventoryPlatformWebApplicationFactory();
        var arrangement = await ArrangeSubmittedAsync(factory);
        using var client = CreateClient(
            factory, TestUserSelectors.InventoryManager, new CookieContainer());

        var details = await client.GetAsync(BuildDetailsPath(arrangement.PurchaseOrderId));
        Assert.Equal(HttpStatusCode.OK, details.StatusCode);
        var form = PurchaseOrderLifecycleFormExtraction.ExtractCancelForm(
            await details.Content.ReadAsStringAsync());
        AssertFormNavigation(form);
        await AdvanceToApprovedAsync(factory, arrangement.PurchaseOrderId);
        var approvedBaseline = await ReadStateAsync(factory, arrangement);
        Assert.Equal(PurchaseOrderStatus.Approved, approvedBaseline.Status);

        var response = await client.PostAsync(form.Action, CreateFormContent(form));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(response.Headers.Location);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains(
            "Only draft or submitted purchase orders can be cancelled.",
            html,
            StringComparison.Ordinal);
        Assert.Contains(
            ">Approved<",
            html.ReplaceLineEndings(string.Empty).Replace(" ", string.Empty));
        var redisplayedReceiveForm =
            PurchaseOrderLifecycleFormExtraction.ExtractReceiveForm(
                html, arrangement.ProductId);
        AssertFormNavigation(redisplayedReceiveForm);
        var actual = await ReadStateAsync(factory, arrangement);
        AssertOnlyStatusChanged(
            approvedBaseline, actual, PurchaseOrderStatus.Approved);
    }

    private static Task<LifecycleArrangement> ArrangeSubmittedAsync(
        InventoryPlatformWebApplicationFactory factory) =>
        ArrangeAsync(factory, approve: false);

    private static Task<LifecycleArrangement> ArrangeApprovedAsync(
        InventoryPlatformWebApplicationFactory factory) =>
        ArrangeAsync(factory, approve: true);

    private static async Task<LifecycleArrangement> ArrangeAsync(
        InventoryPlatformWebApplicationFactory factory, bool approve)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var marker = Guid.NewGuid().ToString("N");
        var supplier = new Supplier($"S23 Supplier {marker}", "Sprint 23",
            $"supplier-{marker}@example.test", null, null);
        var category = new Category($"S23 Category {marker}", null);
        var unit = new Unit($"S23-{marker[..8]}", $"S23 Unit {marker}", "ea");
        context.AddRange(supplier, category, unit);
        await context.SaveChangesAsync();
        var product = new Product($"S23-{marker}", $"S23 Product {marker}",
            category.Id, unit.Id, 7m, 12.50m, 20m);
        context.Products.Add(product);
        await context.SaveChangesAsync();
        var order = PurchaseOrder.Create(supplier.Id, new DateOnly(2026, 9, 1),
            new DateOnly(2026, 10, 1), $"S23 Order {marker}");
        order.AddItem(product.Id, 5m, 12.50m);
        order.Submit();
        if (approve) order.Approve();
        context.PurchaseOrders.Add(order);
        await context.SaveChangesAsync();
        return new(order.Id, product.Id, supplier.Name, product.Sku, product.Name,
            await CaptureAsync(context, order.Id, product.Id));
    }

    private static async Task<ViewOnlyArrangement> ArrangeViewOnlyUserAsync(
        InventoryPlatformWebApplicationFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(), UserName = "purchaseorder-view-only",
            Email = TestUserSelectors.PurchaseOrderDenied, EmailConfirmed = true
        };
        var result = await users.CreateAsync(user);
        Assert.True(result.Succeeded, string.Join(Environment.NewLine,
            result.Errors.Select(error => error.Description)));
        var group = new AuthorizationGroup($"S23 PO View Only {Guid.NewGuid():N}");
        context.AuthorizationGroups.Add(group);
        await context.SaveChangesAsync();
        var view = await context.Capabilities.SingleAsync(capability =>
            capability.Name == AuthorizationPolicies.PurchaseOrder.View);
        group.AddCapability(view);
        group.AssignUser(user.Id);
        await context.SaveChangesAsync();
        Assert.DoesNotContain(group.Capabilities,
            relationship => relationship.CapabilityId != view.Id);
        return new(user.Id, TestUserSelectors.PurchaseOrderDenied, group.Id);
    }

    private static async Task AdvanceToApprovedAsync(
        InventoryPlatformWebApplicationFactory factory, int purchaseOrderId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var order = await context.PurchaseOrders.Include(x => x.Items)
            .SingleAsync(x => x.Id == purchaseOrderId);
        order.Approve();
        await context.SaveChangesAsync();
    }

    private static async Task<LifecycleState> ReadStateAsync(
        InventoryPlatformWebApplicationFactory factory, LifecycleArrangement arrangement)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await CaptureAsync(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
            arrangement.PurchaseOrderId, arrangement.ProductId);
    }

    private static async Task<LifecycleState> CaptureAsync(
        ApplicationDbContext context, int purchaseOrderId, int productId)
    {
        var order = await context.PurchaseOrders.AsNoTracking().Include(x => x.Items)
            .SingleAsync(x => x.Id == purchaseOrderId);
        var product = await context.Products.AsNoTracking().SingleAsync(x => x.Id == productId);
        var transactions = await context.InventoryTransactions.AsNoTracking()
            .Where(x => x.ProductId == productId).OrderBy(x => x.Id)
            .Select(x => new TransactionState(x.Id, x.ProductId, x.TransactionType,
                x.Quantity, x.ReferenceNumber, x.Remarks, x.TransactionDateUtc)).ToArrayAsync();
        return new(order.SupplierId, order.OrderDate, order.ExpectedDeliveryDate,
            order.Remarks, order.Status, order.TotalAmount,
            order.Items.OrderBy(x => x.ProductId)
                .Select(x => new ItemState(x.ProductId, x.Quantity, x.UnitCost,
                    x.ReceivedQuantity)).ToArray(),
            new ProductState(product.Id, product.QuantityOnHand), transactions);
    }

    private static FormUrlEncodedContent CreateFormContent(
        PurchaseOrderLifecycleForm form, params (string Name, string Value)[] overrides)
    {
        var fields = new Dictionary<string, string>(form.Fields);
        foreach (var (name, value) in overrides) fields[name] = value;
        return new FormUrlEncodedContent(fields);
    }

    private static string BuildDetailsPath(int id)
    {
        var n = Navigation;
        return $"{DetailsPrefix}{id}?Search={Uri.EscapeDataString(n.Search)}" +
            $"&FromDate={n.FromDate:yyyy-MM-dd}&ToDate={n.ToDate:yyyy-MM-dd}" +
            $"&Status={n.Status}&SortBy={n.SortBy}&Descending={n.Descending}" +
            $"&PageNum={n.PageNum}&PageSize={n.PageSize}";
    }

    private static HttpClient CreateClient(InventoryPlatformWebApplicationFactory factory,
        string selector, CookieContainer? cookies = null)
    {
        HttpClient client;
        if (cookies is null)
        {
            client = factory.CreateClient(new WebApplicationFactoryClientOptions
            { AllowAutoRedirect = false, BaseAddress = new Uri("https://localhost") });
        }
        else
        {
            var handler = new CookiePreservingHandler(cookies)
            { InnerHandler = factory.Server.CreateHandler() };
            client = new HttpClient(handler) { BaseAddress = new Uri("https://localhost") };
        }
        client.DefaultRequestHeaders.Add(TestAuthenticationDefaults.UserHeader, selector);
        return client;
    }

    private static void AssertDetailsDestination(Uri location, int id)
    {
        var target = Absolute(location);
        Assert.Equal($"{DetailsPrefix}{id}", target.AbsolutePath);
        AssertNavigation(QueryHelpers.ParseQuery(target.Query));
    }

    private static void AssertAccessDeniedDestination(Uri location, string returnUrl)
    {
        var target = Absolute(location);
        Assert.Equal("/Identity/Account/AccessDenied", target.AbsolutePath);
        Assert.Equal(returnUrl, Assert.Single(QueryHelpers.ParseQuery(target.Query)["ReturnUrl"]));
    }

    private static void AssertFormNavigation(PurchaseOrderLifecycleForm form) =>
        AssertNavigation(form.Fields);

    private static void AssertOnlyStatusChanged(
        LifecycleState expected,
        LifecycleState actual,
        PurchaseOrderStatus expectedStatus)
    {
        Assert.Equal(expectedStatus, actual.Status);
        Assert.Equal(expected.SupplierId, actual.SupplierId);
        Assert.Equal(expected.OrderDate, actual.OrderDate);
        Assert.Equal(expected.ExpectedDeliveryDate, actual.ExpectedDeliveryDate);
        Assert.Equal(expected.Remarks, actual.Remarks);
        Assert.Equal(expected.TotalAmount, actual.TotalAmount);
        Assert.Equal(expected.Items, actual.Items);
        Assert.Equal(expected.Product, actual.Product);
        Assert.Equal(expected.Transactions, actual.Transactions);
    }

    private static void AssertNavigation<T>(IReadOnlyDictionary<string, T> values)
    {
        static string Value(object value) => value.ToString()!;
        Assert.Equal(Navigation.Search, Value(values["Search"]!));
        Assert.Equal(
            Navigation.FromDate,
            ParseNavigationDate(Value(values["FromDate"]!)));
        Assert.Equal(
            Navigation.ToDate,
            ParseNavigationDate(Value(values["ToDate"]!)));
        Assert.Equal(Navigation.Status, Value(values["Status"]!));
        Assert.Equal(Navigation.SortBy, Value(values["SortBy"]!));
        Assert.Equal(Navigation.Descending.ToString(), Value(values["Descending"]!), true);
        Assert.Equal(Navigation.PageNum.ToString(CultureInfo.InvariantCulture), Value(values["PageNum"]!));
        Assert.Equal(Navigation.PageSize.ToString(CultureInfo.InvariantCulture), Value(values["PageSize"]!));
    }

    private static DateOnly ParseNavigationDate(string value) =>
        DateOnly.ParseExact(
            value,
            ["yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy"],
            CultureInfo.InvariantCulture);

    private static Uri Absolute(Uri location) => location.IsAbsoluteUri
        ? location : new Uri(new Uri("https://localhost"), location);

    private sealed record LifecycleNavigation(string Search, DateOnly FromDate,
        DateOnly ToDate, string Status, string SortBy, bool Descending,
        int PageNum, int PageSize);
    private sealed record LifecycleArrangement(int PurchaseOrderId, int ProductId,
        string SupplierName, string ProductSku, string ProductName, LifecycleState InitialState);
    private sealed record ViewOnlyArrangement(Guid UserId, string Selector, int GroupId);
    private sealed record LifecycleState(int SupplierId, DateOnly OrderDate,
        DateOnly? ExpectedDeliveryDate, string? Remarks, PurchaseOrderStatus Status,
        decimal TotalAmount, IReadOnlyList<ItemState> Items, ProductState Product,
        IReadOnlyList<TransactionState> Transactions);
    private sealed record ItemState(int ProductId, decimal Quantity,
        decimal UnitCost, decimal ReceivedQuantity);
    private sealed record ProductState(int ProductId, decimal QuantityOnHand);
    private sealed record TransactionState(int Id, int ProductId,
        TransactionType TransactionType, decimal Quantity, string ReferenceNumber,
        string? Remarks, DateTime TransactionDateUtc);

    private sealed class CookiePreservingHandler(CookieContainer cookies) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var header = cookies.GetCookieHeader(request.RequestUri!);
            if (header.Length > 0) request.Headers.TryAddWithoutValidation("Cookie", header);
            var response = await base.SendAsync(request, cancellationToken);
            if (response.Headers.TryGetValues("Set-Cookie", out var values))
                foreach (var value in values) cookies.SetCookies(request.RequestUri!, value);
            return response;
        }
    }
}
