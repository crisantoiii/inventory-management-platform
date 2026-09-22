using System.Globalization;
using System.Net;
using System.Net.Http;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Web.Tests.Authentication;
using InventoryPlatform.Web.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InventoryPlatform.Web.Tests.Http;

/// <summary>
/// Sprint 17 T07 — Purchase Order domain-failure HTTP proof.
///
/// Proves through the real authenticated HTTPS Razor pipeline that submitting
/// an empty Draft Purchase Order reaches the actual production
/// Application/Domain path, hits the real domain invariant (a purchase order
/// must contain at least one item), and is presented by the existing
/// production failure redisplay — with the relevant navigation state preserved
/// and no unintended PO mutation.
///
/// The real seeded InventoryManager authenticates, the real capability
/// authorization infrastructure authorizes the Submit POST, the real rendered
/// antiforgery token and its matching preserved cookie are consumed, real
/// model binding and the real PageModel/Application handler/Domain aggregate
/// execute, and the same isolated factory database proves no mutation. No
/// handler or aggregate is invoked directly; no authorization, claim, or
/// exception is fabricated; antiforgery is never disabled, bypassed, or
/// manufactured; no parser package is added.
///
/// The empty Draft prerequisite is arranged directly through the isolated test
/// database (real Domain factory + context + SaveChanges) — the narrowest
/// deterministic arrangement, since the behavior under test is Submit, not
/// Create.
/// </summary>
public sealed class PurchaseOrderSubmitDomainFailureHttpTests
{
    private const string PurchaseOrderDetailsPathPrefix =
        "/Purchasing/PurchaseOrders/Details/";

    private const string AntiforgeryCookieName = "InventoryPlatform.AntiForgery";

    // Deliberately non-default, representative navigation values — the
    // production contract for a domain-failure redisplay is to preserve the
    // navigation/query state the Submit POST carried.
    private const string NavigationSearch = "T07-Nav-Search";

    private const string NavigationSortBy = "OrderDate";

    private const string NavigationStatus = "Draft";

    private const string NavigationFromDate = "2026-01-01";

    private const string NavigationToDate = "2026-12-31";

    private const int NavigationPageNum = 2;

    private const int NavigationPageSize = 25;

    [Fact]
    public async Task PostSubmit_AsInventoryManager_OnEmptyDraft_RendersDomainFailureAndPreservesNavigationStateWithoutMutation()
    {
        // ================================================================
        // A. Arrange — a valid, production-representative Draft Purchase
        //    Order with zero items, in the isolated factory database.
        // ================================================================
        await using var factory = new InventoryPlatformWebApplicationFactory();

        var (purchaseOrderId, initialState) =
            await ArrangeEmptyDraftPurchaseOrderAsync(factory);

        var cookieContainer = new CookieContainer();

        using var client = CreateClient(factory, cookieContainer);

        // ================================================================
        // B. GET the real rendered Details page with deliberate
        //    non-default navigation/query state.
        // ================================================================
        var detailsPathWithNavigationState =
            $"{PurchaseOrderDetailsPathPrefix}{purchaseOrderId}" +
            $"?Search={Uri.EscapeDataString(NavigationSearch)}" +
            $"&FromDate={NavigationFromDate}" +
            $"&ToDate={NavigationToDate}" +
            $"&Status={NavigationStatus}" +
            $"&SortBy={NavigationSortBy}" +
            $"&Descending=true" +
            $"&PageNum={NavigationPageNum}" +
            $"&PageSize={NavigationPageSize}";

        var getResponse = await client.GetAsync(detailsPathWithNavigationState);

        var getHtml = await getResponse.Content.ReadAsStringAsync();

        Assert.True(
            getResponse.StatusCode == HttpStatusCode.OK,
            $"Expected HTTP 200 on GET {detailsPathWithNavigationState} but received " +
            $"{(int)getResponse.StatusCode}.{Environment.NewLine}{getHtml}");

        // Consume the real rendered Submit form (identified by its
        // production button text among the page's several forms).
        var submitForm = PurchaseOrderSubmitFormExtraction
            .ExtractSubmitForm(getHtml);

        // The effective action targets this PO's Details route with the
        // real handler selector rendered by the asp-page-handler tag helper.
        Assert.Equal(
            $"{PurchaseOrderDetailsPathPrefix}{purchaseOrderId}?handler=Submit",
            submitForm.Action);

        // The real rendered hidden fields must carry the deliberate
        // navigation state (production contract: the Submit form round-trips
        // the navigation state it was rendered with). The FromDate/ToDate
        // hidden inputs are rendered with the host culture's short-date
        // pattern, so they are asserted semantically: the rendered value must
        // parse back — under the same culture the in-process server rendered
        // it with and its model binder parses it with — to the exact date the
        // page was requested with.
        Assert.Equal(
            purchaseOrderId.ToString(CultureInfo.InvariantCulture),
            submitForm.HiddenFields["id"]);
        Assert.Equal(NavigationSearch, submitForm.HiddenFields["Search"]);
        Assert.Equal(
            new DateOnly(2026, 1, 1),
            DateOnly.Parse(submitForm.HiddenFields["FromDate"]));
        Assert.Equal(
            new DateOnly(2026, 12, 31),
            DateOnly.Parse(submitForm.HiddenFields["ToDate"]));
        Assert.Equal(NavigationStatus, submitForm.HiddenFields["Status"]);
        Assert.Equal(NavigationSortBy, submitForm.HiddenFields["SortBy"]);
        Assert.Equal("true", submitForm.HiddenFields["Descending"]);
        Assert.Equal(
            NavigationPageNum.ToString(CultureInfo.InvariantCulture),
            submitForm.HiddenFields["PageNum"]);
        Assert.Equal(
            NavigationPageSize.ToString(CultureInfo.InvariantCulture),
            submitForm.HiddenFields["PageSize"]);

        // Consume the real rendered antiforgery hidden token.
        var antiforgeryToken =
            submitForm.HiddenFields["__RequestVerificationToken"];

        Assert.False(
            string.IsNullOrWhiteSpace(antiforgeryToken),
            "The rendered antiforgery token must not be empty.");

        // Prove the matching antiforgery cookie was preserved by the client.
        var preservedAntiforgeryCookie = GetAntiforgeryCookie(
            cookieContainer,
            client.BaseAddress!);

        Assert.False(
            string.IsNullOrWhiteSpace(preservedAntiforgeryCookie.Value),
            "The antiforgery cookie emitted by the GET response must have a value.");

        // ================================================================
        // C. POST Submit to the effective form action with the real token,
        //    matching cookie, required PO identifier, and the actual
        //    navigation-state fields — exactly as the rendered form would.
        // ================================================================
        using var postRequest = new HttpRequestMessage(
            HttpMethod.Post,
            submitForm.Action)
        {
            Content = new FormUrlEncodedContent(submitForm.HiddenFields)
        };

        var postResponse = await client.SendAsync(postRequest);

        var postHtml = await postResponse.Content.ReadAsStringAsync();

        // ================================================================
        // D. Verify the actual production domain-failure redisplay.
        //    Production catches the DomainException, adds it to ModelState,
        //    reloads the PO, and re-renders the Details page (HTTP 200).
        // ================================================================
        Assert.True(
            postResponse.StatusCode == HttpStatusCode.OK,
            $"Expected the production domain-failure redisplay (HTTP 200) but received " +
            $"{(int)postResponse.StatusCode}.{Environment.NewLine}{postHtml}");

        // The actual production user-visible error text from the real
        // Domain invariant.
        Assert.Contains(
            "A purchase order must contain at least one item.",
            postHtml,
            StringComparison.Ordinal);

        // Relevant PO context is preserved in the redisplay.
        Assert.Contains(
            "Purchase Order Details",
            postHtml,
            StringComparison.Ordinal);
        Assert.Contains(
            initialState.SupplierName,
            postHtml,
            StringComparison.Ordinal);

        // The redisplayed page must still offer the Draft actions (the PO
        // context was not lost).
        Assert.Contains(
            "Submit Purchase Order",
            postHtml,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "was submitted successfully",
            postHtml,
            StringComparison.Ordinal);

        // ================================================================
        // E. Verify no mutation — fresh DI scope from the same factory.
        // ================================================================
        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var reloaded = await dbContext.PurchaseOrders
            .AsNoTracking()
            .Include(order => order.Items)
            .SingleAsync(
                order => order.Id == purchaseOrderId);

        // The PO did not transition out of Draft.
        Assert.Equal(PurchaseOrderStatus.Draft, reloaded.Status);

        // Item count remains zero.
        Assert.Empty(reloaded.Items);

        // Submit-specific values remain unchanged.
        Assert.Equal(initialState.SupplierId, reloaded.SupplierId);
        Assert.Equal(initialState.OrderDate, reloaded.OrderDate);
        Assert.Equal(
            initialState.ExpectedDeliveryDate,
            reloaded.ExpectedDeliveryDate);
        Assert.Equal(initialState.Remarks, reloaded.Remarks);
        Assert.Equal(0m, reloaded.TotalAmount);
    }

    /// <summary>
    /// Proves the domain-failure redisplay preserves the actual
    /// navigation/query state production intends to survive a Submit failure:
    /// the redisplayed Submit form must round-trip every navigation field with
    /// the exact values the failed POST carried.
    /// </summary>
    [Fact]
    public async Task PostSubmit_OnEmptyDraft_RedisplayedFormRoundTripsNavigationState()
    {
        await using var factory = new InventoryPlatformWebApplicationFactory();

        var (purchaseOrderId, _) =
            await ArrangeEmptyDraftPurchaseOrderAsync(factory);

        var cookieContainer = new CookieContainer();

        using var client = CreateClient(factory, cookieContainer);

        var detailsPathWithNavigationState =
            $"{PurchaseOrderDetailsPathPrefix}{purchaseOrderId}" +
            $"?Search={Uri.EscapeDataString(NavigationSearch)}" +
            $"&FromDate={NavigationFromDate}" +
            $"&ToDate={NavigationToDate}" +
            $"&Status={NavigationStatus}" +
            $"&SortBy={NavigationSortBy}" +
            $"&Descending=true" +
            $"&PageNum={NavigationPageNum}" +
            $"&PageSize={NavigationPageSize}";

        var getResponse = await client.GetAsync(detailsPathWithNavigationState);

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var submitForm = PurchaseOrderSubmitFormExtraction
            .ExtractSubmitForm(
                await getResponse.Content.ReadAsStringAsync());

        using var postRequest = new HttpRequestMessage(
            HttpMethod.Post,
            submitForm.Action)
        {
            Content = new FormUrlEncodedContent(submitForm.HiddenFields)
        };

        var postResponse = await client.SendAsync(postRequest);

        var postHtml = await postResponse.Content.ReadAsStringAsync();

        Assert.True(
            postResponse.StatusCode == HttpStatusCode.OK,
            $"Expected the production domain-failure redisplay (HTTP 200) but received " +
            $"{(int)postResponse.StatusCode}.{Environment.NewLine}{postHtml}");

        Assert.Contains(
            "A purchase order must contain at least one item.",
            postHtml,
            StringComparison.Ordinal);

        // The failure redisplay re-renders the Submit form with the same
        // navigation values the failed POST carried.
        var redisplayedSubmitForm = PurchaseOrderSubmitFormExtraction
            .ExtractSubmitForm(postHtml);

        Assert.Equal(
            $"{PurchaseOrderDetailsPathPrefix}{purchaseOrderId}?handler=Submit",
            redisplayedSubmitForm.Action);
        Assert.Equal(
            purchaseOrderId.ToString(CultureInfo.InvariantCulture),
            redisplayedSubmitForm.HiddenFields["id"]);
        Assert.Equal(NavigationSearch, redisplayedSubmitForm.HiddenFields["Search"]);
        Assert.Equal(
            new DateOnly(2026, 1, 1),
            DateOnly.Parse(redisplayedSubmitForm.HiddenFields["FromDate"]));
        Assert.Equal(
            new DateOnly(2026, 12, 31),
            DateOnly.Parse(redisplayedSubmitForm.HiddenFields["ToDate"]));
        Assert.Equal(NavigationStatus, redisplayedSubmitForm.HiddenFields["Status"]);
        Assert.Equal(NavigationSortBy, redisplayedSubmitForm.HiddenFields["SortBy"]);
        Assert.Equal("true", redisplayedSubmitForm.HiddenFields["Descending"]);
        Assert.Equal(
            NavigationPageNum.ToString(CultureInfo.InvariantCulture),
            redisplayedSubmitForm.HiddenFields["PageNum"]);
        Assert.Equal(
            NavigationPageSize.ToString(CultureInfo.InvariantCulture),
            redisplayedSubmitForm.HiddenFields["PageSize"]);
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    private static async Task<(int PurchaseOrderId, InitialPurchaseOrderState State)>
        ArrangeEmptyDraftPurchaseOrderAsync(
            InventoryPlatformWebApplicationFactory factory)
    {
        // A fresh DI scope from the same factory arranges the prerequisite
        // through the real Domain factory + context + SaveChanges — no direct
        // table manipulation, no shared/static state, unique data.
        using var scope = factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var supplier = new Supplier(
            $"T07 Supplier {Guid.NewGuid():N}",
            contactPerson: null,
            email: null,
            phone: null,
            address: null);

        dbContext.Suppliers.Add(supplier);

        var purchaseOrder = PurchaseOrder.Create(
            supplier.Id,
            new DateOnly(2026, 9, 1),
            expectedDeliveryDate: new DateOnly(2026, 10, 1),
            remarks: $"T07 empty Draft {Guid.NewGuid():N}");

        // Zero items — the empty-Draft prerequisite for the domain invariant.
        dbContext.PurchaseOrders.Add(purchaseOrder);

        await dbContext.SaveChangesAsync();

        return (
            purchaseOrder.Id,
            new InitialPurchaseOrderState(
                SupplierId: supplier.Id,
                SupplierName: supplier.Name,
                OrderDate: purchaseOrder.OrderDate,
                ExpectedDeliveryDate: purchaseOrder.ExpectedDeliveryDate,
                Remarks: purchaseOrder.Remarks));
    }

    private static HttpClient CreateClient(
        InventoryPlatformWebApplicationFactory factory,
        CookieContainer cookieContainer)
    {
        // A narrow cookie-preserving handler wraps the factory's request-
        // creating handler: the antiforgery cookie emitted by the GET
        // response is preserved and sent with the POST, exactly as a browser
        // would. Redirects are not followed, so the failure redisplay
        // response from the POST itself is observable. No antiforgery
        // services are touched and no token is manufactured.
        var cookieHandler = new CookiePreservingHandler(cookieContainer)
        {
            InnerHandler = factory.Server.CreateHandler()
        };

        var client = new HttpClient(cookieHandler)
        {
            BaseAddress = new Uri("https://localhost"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        // The real seeded InventoryManager selector is kept across GET and
        // POST: real T04 seeded authentication with a real GUID identity.
        client.DefaultRequestHeaders.Add(
            TestAuthenticationDefaults.UserHeader,
            TestUserSelectors.InventoryManager);

        return client;
    }

    private static Cookie GetAntiforgeryCookie(
        CookieContainer cookieContainer,
        Uri baseAddress)
    {
        var antiforgeryCookies = cookieContainer
            .GetCookies(baseAddress)
            .Cast<Cookie>()
            .Where(cookie => cookie.Name == AntiforgeryCookieName)
            .ToList();

        return Assert.Single(antiforgeryCookies);
    }

    private sealed record InitialPurchaseOrderState(
        int SupplierId,
        string SupplierName,
        DateOnly OrderDate,
        DateOnly? ExpectedDeliveryDate,
        string? Remarks);

    /// <summary>
    /// Narrow BCL cookie-preserving handler: replays preserved cookies onto
    /// outgoing requests and stores Set-Cookie headers from responses. No
    /// antiforgery, authentication, or redirect behavior.
    /// </summary>
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
                request.Headers.TryAddWithoutValidation(
                    "Cookie",
                    cookieHeader);
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
