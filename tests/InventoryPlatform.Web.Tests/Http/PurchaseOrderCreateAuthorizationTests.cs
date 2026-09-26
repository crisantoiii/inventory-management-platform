using System.Net;
using System.Text.RegularExpressions;
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

public sealed class PurchaseOrderCreateAuthorizationTests
{
    private const string PurchaseOrderCreatePath =
        "/Purchasing/PurchaseOrders/Create";

    private const string PurchaseOrderIndexPath =
        "/Purchasing/PurchaseOrders";

    private const string AntiforgeryCookieName =
        "InventoryPlatform.AntiForgery";

    private const string AntiforgeryTokenFieldName =
        "__RequestVerificationToken";

    private const string ValidCreateRemarks =
        "S19-T03-VALID-CREATE";

    private const string DuplicateCreateRemarks =
        "S19-T04-DUPLICATE";

    private const string DuplicateProductMessage =
        "The product already exists in this purchase order.";

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

    [Fact]
    public async Task PostPurchaseOrderCreate_AsInventoryManager_WithRenderedAntiforgeryMaterial_PersistsOrderAndRedirects()
    {
        await using var factory = new InventoryPlatformWebApplicationFactory();
        var arrangedData = await ArrangePurchaseOrderOptionsAsync(factory);
        var cookieContainer = new CookieContainer();
        using var client = CreateCookiePreservingClient(factory, cookieContainer);

        var getResponse = await client.GetAsync(PurchaseOrderCreatePath);
        var getHtml = await getResponse.Content.ReadAsStringAsync();

        Assert.True(
            getResponse.StatusCode == HttpStatusCode.OK,
            $"Expected HTTP 200 on GET {PurchaseOrderCreatePath} but received " +
            $"{(int)getResponse.StatusCode}.{Environment.NewLine}{getHtml}");

        var formAction = PurchaseOrderCreateFormExtraction
            .ExtractCreateFormAction(getHtml);
        var antiforgeryToken = PurchaseOrderCreateFormExtraction
            .ExtractCreateFormAntiforgeryToken(getHtml);

        Assert.Equal(PurchaseOrderCreatePath, formAction);
        Assert.False(string.IsNullOrWhiteSpace(antiforgeryToken));

        var antiforgeryCookie = Assert.Single(
            cookieContainer
                .GetCookies(client.BaseAddress!)
                .Cast<Cookie>(),
            cookie => cookie.Name == AntiforgeryCookieName);

        Assert.False(string.IsNullOrWhiteSpace(antiforgeryCookie.Value));

        using var postRequest = new HttpRequestMessage(HttpMethod.Post, formAction)
        {
            Content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    [AntiforgeryTokenFieldName] = antiforgeryToken,
                    ["PurchaseOrder.SupplierId"] = arrangedData.SupplierId.ToString(),
                    ["PurchaseOrder.ExpectedDeliveryDate"] = "2030-01-15",
                    ["PurchaseOrder.Remarks"] = ValidCreateRemarks,
                    ["PurchaseOrder.Items[0].ProductId"] = arrangedData.ProductId.ToString(),
                    ["PurchaseOrder.Items[0].Quantity"] = "3",
                    ["PurchaseOrder.Items[0].UnitCost"] = "12.50"
                })
        };

        var postResponse = await client.SendAsync(postRequest);
        var postHtml = await postResponse.Content.ReadAsStringAsync();

        Assert.True(
            postResponse.StatusCode == HttpStatusCode.Redirect,
            $"Expected HTTP 302 but received {(int)postResponse.StatusCode}." +
            $"{Environment.NewLine}{postHtml}");

        var location = Assert.IsType<Uri>(postResponse.Headers.Location);
        var destination = location.IsAbsoluteUri
            ? location
            : new Uri(client.BaseAddress!, location);

        Assert.Equal(PurchaseOrderIndexPath, destination.AbsolutePath);

        await using var verificationScope = factory.Services.CreateAsyncScope();
        var context = verificationScope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        var persistedOrder = await context.PurchaseOrders
            .AsNoTracking()
            .Include(order => order.Items)
            .SingleAsync(order => order.Remarks == ValidCreateRemarks);

        Assert.Equal(arrangedData.SupplierId, persistedOrder.SupplierId);
        Assert.Equal(new DateOnly(2030, 1, 15), persistedOrder.ExpectedDeliveryDate);
        Assert.Equal(ValidCreateRemarks, persistedOrder.Remarks);
        Assert.Equal(PurchaseOrderStatus.Draft, persistedOrder.Status);

        var persistedItem = Assert.Single(persistedOrder.Items);
        Assert.Equal(arrangedData.ProductId, persistedItem.ProductId);
        Assert.Equal(3m, persistedItem.Quantity);
        Assert.Equal(12.50m, persistedItem.UnitCost);
        Assert.Equal(37.50m, persistedItem.LineTotal);
    }

    [Fact]
    public async Task PostPurchaseOrderCreate_AsInventoryManager_WithDuplicateProduct_RedisplaysRestoredFormWithoutMutation()
    {
        await using var factory = new InventoryPlatformWebApplicationFactory();
        var arrangedData = await ArrangePurchaseOrderOptionsAsync(factory);
        var cookieContainer = new CookieContainer();
        using var client = CreateCookiePreservingClient(factory, cookieContainer);

        var getResponse = await client.GetAsync(PurchaseOrderCreatePath);
        var getHtml = await getResponse.Content.ReadAsStringAsync();

        Assert.True(
            getResponse.StatusCode == HttpStatusCode.OK,
            $"Expected HTTP 200 on GET {PurchaseOrderCreatePath} but received " +
            $"{(int)getResponse.StatusCode}.{Environment.NewLine}{getHtml}");

        var formAction = PurchaseOrderCreateFormExtraction
            .ExtractCreateFormAction(getHtml);
        var antiforgeryToken = PurchaseOrderCreateFormExtraction
            .ExtractCreateFormAntiforgeryToken(getHtml);

        Assert.Equal(PurchaseOrderCreatePath, formAction);
        Assert.False(string.IsNullOrWhiteSpace(antiforgeryToken));

        var antiforgeryCookie = Assert.Single(
            cookieContainer
                .GetCookies(client.BaseAddress!)
                .Cast<Cookie>(),
            cookie => cookie.Name == AntiforgeryCookieName);

        Assert.False(string.IsNullOrWhiteSpace(antiforgeryCookie.Value));

        int initialOrderCount;
        int initialItemCount;

        await using (var initialScope = factory.Services.CreateAsyncScope())
        {
            var context = initialScope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            initialOrderCount = await context.PurchaseOrders.CountAsync();
            initialItemCount = await context.PurchaseOrderItems.CountAsync();
            Assert.False(await context.PurchaseOrders.AnyAsync(
                order => order.Remarks == DuplicateCreateRemarks));
        }

        using var postRequest = new HttpRequestMessage(HttpMethod.Post, formAction)
        {
            Content = new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    [AntiforgeryTokenFieldName] = antiforgeryToken,
                    ["PurchaseOrder.SupplierId"] = arrangedData.SupplierId.ToString(),
                    ["PurchaseOrder.ExpectedDeliveryDate"] = "2030-01-15",
                    ["PurchaseOrder.Remarks"] = DuplicateCreateRemarks,
                    ["PurchaseOrder.Items[0].ProductId"] = arrangedData.ProductId.ToString(),
                    ["PurchaseOrder.Items[0].Quantity"] = "2",
                    ["PurchaseOrder.Items[0].UnitCost"] = "12.50",
                    ["PurchaseOrder.Items[1].ProductId"] = arrangedData.ProductId.ToString(),
                    ["PurchaseOrder.Items[1].Quantity"] = "4",
                    ["PurchaseOrder.Items[1].UnitCost"] = "15.75"
                })
        };

        var postResponse = await client.SendAsync(postRequest);
        var postHtml = await postResponse.Content.ReadAsStringAsync();

        Assert.True(
            postResponse.StatusCode == HttpStatusCode.OK,
            $"Expected HTTP 200 domain-failure redisplay but received " +
            $"{(int)postResponse.StatusCode}.{Environment.NewLine}{postHtml}");
        Assert.Null(postResponse.Headers.Location);
        Assert.Contains(
            DuplicateProductMessage,
            PurchaseOrderCreateFormExtraction.ExtractValidationSummaryText(postHtml),
            StringComparison.Ordinal);

        AssertSelectContainsOption(
            postHtml,
            "PurchaseOrder.SupplierId",
            arrangedData.SupplierId,
            "S19 Purchase Order Supplier");
        AssertSelectContainsOption(
            postHtml,
            "PurchaseOrder.Items[0].ProductId",
            arrangedData.ProductId,
            "S19 Purchase Order Product");
        AssertSelectContainsOption(
            postHtml,
            "PurchaseOrder.Items[1].ProductId",
            arrangedData.ProductId,
            "S19 Purchase Order Product");

        Assert.Equal(
            arrangedData.SupplierId.ToString(),
            PurchaseOrderCreateFormExtraction.ExtractSelectedOptionValue(
                postHtml,
                "PurchaseOrder.SupplierId"));
        Assert.Equal(
            "2030-01-15",
            PurchaseOrderCreateFormExtraction.ExtractInputValue(
                postHtml,
                "PurchaseOrder.ExpectedDeliveryDate"));
        Assert.Equal(
            DuplicateCreateRemarks,
            PurchaseOrderCreateFormExtraction.ExtractTextareaValue(
                postHtml,
                "PurchaseOrder.Remarks"));
        Assert.Equal(2, PurchaseOrderCreateFormExtraction.CountItemRows(postHtml));

        Assert.Equal(
            arrangedData.ProductId.ToString(),
            PurchaseOrderCreateFormExtraction.ExtractSelectedOptionValue(
                postHtml,
                "PurchaseOrder.Items[0].ProductId"));
        Assert.Equal(
            "2",
            PurchaseOrderCreateFormExtraction.ExtractInputValue(
                postHtml,
                "PurchaseOrder.Items[0].Quantity"));
        Assert.Equal(
            "12.50",
            PurchaseOrderCreateFormExtraction.ExtractInputValue(
                postHtml,
                "PurchaseOrder.Items[0].UnitCost"));
        Assert.Equal(
            arrangedData.ProductId.ToString(),
            PurchaseOrderCreateFormExtraction.ExtractSelectedOptionValue(
                postHtml,
                "PurchaseOrder.Items[1].ProductId"));
        Assert.Equal(
            "4",
            PurchaseOrderCreateFormExtraction.ExtractInputValue(
                postHtml,
                "PurchaseOrder.Items[1].Quantity"));
        Assert.Equal(
            "15.75",
            PurchaseOrderCreateFormExtraction.ExtractInputValue(
                postHtml,
                "PurchaseOrder.Items[1].UnitCost"));

        await using var verificationScope = factory.Services.CreateAsyncScope();
        var verificationContext = verificationScope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        Assert.Equal(initialOrderCount, await verificationContext.PurchaseOrders.CountAsync());
        Assert.Equal(initialItemCount, await verificationContext.PurchaseOrderItems.CountAsync());
        Assert.False(await verificationContext.PurchaseOrders.AnyAsync(
            order => order.Remarks == DuplicateCreateRemarks));
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

    private static HttpClient CreateCookiePreservingClient(
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
