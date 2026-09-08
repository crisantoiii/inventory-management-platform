using InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrders;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.UnitTests.TestSupport.Purchasing;
using Xunit;

namespace InventoryPlatform.UnitTests.Application.Purchasing;

/// <summary>
/// T05 unit tests for <see cref="GetPurchaseOrdersHandler"/>.
///
/// Scope: Application-handler request construction (pass-through) and response mapping
/// against the shared T01 <see cref="FakePurchaseOrderRepository"/>, which records every
/// <c>GetPagedAsync</c> argument. These tests do NOT verify real EF Core filtering,
/// sorting, paging, or SQL translation — the fake returns prepared
/// <see cref="PagedResult{PurchaseOrder}"/> values. Real repository query behavior
/// belongs to T06 (PurchaseOrderRepository integration tests).
///
/// Current-source fact preserved: the handler has NO business failure branch — it maps
/// the repository result into a successful response unconditionally. No failure path is
/// asserted into existence.
///
/// Navigation fixtures: the handler dereferences <c>po.Supplier.Name</c>, and
/// <see cref="PurchaseOrder.Supplier"/> has a private setter with no public attach API
/// (EF Core populates it). Per the accepted Navigation Fixture Rules, the smallest
/// test-only mechanism is used: a reflection helper private to this file (mirroring the
/// <see cref="EntityIdHelper"/> precedent). No production visibility or setter was
/// changed.
/// </summary>
public sealed class GetPurchaseOrdersHandlerTests
{
    // =====================================================================
    // Request pass-through
    // =====================================================================

    [Fact]
    public async Task HandleAsync_NonDefaultRequest_PassesAllArgumentsToRepository()
    {
        // Arrange — non-default values for every pass-through argument.
        var fromDate = new DateOnly(2026, 1, 1);
        var toDate = new DateOnly(2026, 6, 30);

        var request = new GetPurchaseOrdersRequest
        {
            PageNum = 3,
            PageSize = 25,
            Search = "widget",
            SortBy = "Supplier",
            Descending = true,
            FromDate = fromDate,
            ToDate = toDate,
            PurchaseOrderStatus = PurchaseOrderStatus.Approved
        };

        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var handler = new GetPurchaseOrdersHandler(purchaseOrderRepository);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal(1, purchaseOrderRepository.GetPagedAsyncCallCount);

        var call = purchaseOrderRepository.LastGetPagedAsyncCall!;

        // The handler constructs a PagedQuery from the request's paging fields.
        Assert.Equal(3, call.Query.PageNum);
        Assert.Equal(25, call.Query.PageSize);
        Assert.Equal("widget", call.Query.Search);
        Assert.Equal("Supplier", call.Query.SortBy);
        Assert.True(call.Query.Descending);

        // Filters and the cancellation token are forwarded as-is.
        Assert.Equal(fromDate, call.FromDate);
        Assert.Equal(toDate, call.ToDate);
        Assert.Equal(PurchaseOrderStatus.Approved, call.Status);
    }

    [Fact]
    public async Task HandleAsync_NullFilters_PassesNullFiltersToRepository()
    {
        // Arrange
        var request = new GetPurchaseOrdersRequest
        {
            PageNum = 1,
            PageSize = 10,
            Search = null,
            SortBy = null,
            Descending = false,
            FromDate = null,
            ToDate = null,
            PurchaseOrderStatus = null
        };

        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var handler = new GetPurchaseOrdersHandler(purchaseOrderRepository);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var call = purchaseOrderRepository.LastGetPagedAsyncCall!;
        Assert.Null(call.Query.Search);
        Assert.Null(call.Query.SortBy);
        Assert.False(call.Query.Descending);
        Assert.Null(call.FromDate);
        Assert.Null(call.ToDate);
        Assert.Null(call.Status);
    }

    [Fact]
    public async Task HandleAsync_PreservesCanonicalPageNumContract_ForSubOnePageNum()
    {
        // Arrange — PagedRequest clamps PageNum < 1 to 1 (canonical contract); the
        // handler must forward the clamped value, not the raw request value.
        var request = new GetPurchaseOrdersRequest
        {
            PageNum = 0,
            PageSize = 10
        };

        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var handler = new GetPurchaseOrdersHandler(purchaseOrderRepository);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var call = purchaseOrderRepository.LastGetPagedAsyncCall!;
        Assert.Equal(1, request.PageNum);
        Assert.Equal(1, call.Query.PageNum);
    }

    [Fact]
    public async Task HandleAsync_ClampsOversizedPageSize_ForwardingTheClampedValue()
    {
        // Arrange — PagedRequest clamps PageSize > 100 to 100; the handler forwards the
        // clamped value.
        var request = new GetPurchaseOrdersRequest
        {
            PageNum = 1,
            PageSize = 500
        };

        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var handler = new GetPurchaseOrdersHandler(purchaseOrderRepository);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var call = purchaseOrderRepository.LastGetPagedAsyncCall!;
        Assert.Equal(100, request.PageSize);
        Assert.Equal(100, call.Query.PageSize);
    }

    // =====================================================================
    // Summary mapping
    // =====================================================================

    [Fact]
    public async Task HandleAsync_PopulatedPage_MapsSummariesWithSupplierNavigation()
    {
        // Arrange — a PagedResult of aggregates with populated Supplier navigation
        // (the handler dereferences po.Supplier.Name).
        var first = CreateSummaryPurchaseOrder(
            id: 7,
            supplierId: 3,
            supplierName: "Acme Supplies",
            items: new[] { (ProductId: 10, Quantity: 5m, UnitCost: 25.00m) });                 // Total 125.00
        var second = CreateSummaryPurchaseOrder(
            id: 8,
            supplierId: 4,
            supplierName: "Globex GmbH",
            items: new[] { (ProductId: 10, Quantity: 5m, UnitCost: 25.00m), (ProductId: 20, Quantity: 3m, UnitCost: 10.00m) }); // Total 155.00

        var pagedResult = new PagedResult<PurchaseOrder>
        {
            Items = new[] { first, second },
            Page = 3,
            PageSize = 25,
            TotalCount = 42
        };

        var purchaseOrderRepository = new FakePurchaseOrderRepository(pagedResult: pagedResult);
        var handler = new GetPurchaseOrdersHandler(purchaseOrderRepository);

        var request = new GetPurchaseOrdersRequest
        {
            PageNum = 3,
            PageSize = 25
        };

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var page = result.Value!.PurchaseOrders;

        Assert.Equal(2, page.Items.Count);

        var firstSummary = page.Items[0];
        Assert.Equal(7, firstSummary.Id);
        Assert.Equal("Acme Supplies", firstSummary.SupplierName);
        Assert.Equal(PurchasingTestData.DefaultOrderDate, firstSummary.OrderDate);
        Assert.Equal(PurchaseOrderStatus.Draft, firstSummary.Status);
        Assert.Equal(125.00m, firstSummary.TotalAmount);

        var secondSummary = page.Items[1];
        Assert.Equal(8, secondSummary.Id);
        Assert.Equal("Globex GmbH", secondSummary.SupplierName);
        Assert.Equal(PurchasingTestData.DefaultOrderDate, secondSummary.OrderDate);
        Assert.Equal(PurchaseOrderStatus.Draft, secondSummary.Status);
        Assert.Equal(155.00m, secondSummary.TotalAmount);
    }

    [Fact]
    public async Task HandleAsync_SummaryTotalAmountReflectsAggregateLineTotals()
    {
        // Arrange — 5 * 25 = 125 and 3 * 10 = 30 sum to 155.
        var purchaseOrder = CreateSummaryPurchaseOrder(
            id: 7,
            supplierId: 3,
            supplierName: "Acme Supplies",
            items: new[] { (ProductId: 10, Quantity: 5m, UnitCost: 25.00m), (ProductId: 20, Quantity: 3m, UnitCost: 10.00m) });

        var pagedResult = new PagedResult<PurchaseOrder>
        {
            Items = new[] { purchaseOrder },
            Page = 1,
            PageSize = 10,
            TotalCount = 1
        };

        var purchaseOrderRepository = new FakePurchaseOrderRepository(pagedResult: pagedResult);
        var handler = new GetPurchaseOrdersHandler(purchaseOrderRepository);

        var request = new GetPurchaseOrdersRequest
        {
            PageNum = 1,
            PageSize = 10
        };

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(155.00m, result.Value!.PurchaseOrders.Items.Single().TotalAmount);
    }

    // =====================================================================
    // Paged metadata mapping
    // =====================================================================

    [Fact]
    public async Task HandleAsync_PopulatedPage_CopiesPagingMetadataIntoResponse()
    {
        // Arrange — the repository's own Page/PageSize/TotalCount values are copied
        // verbatim (the response page is keyed by the repository's Page, not the
        // request's PageNum).
        var purchaseOrder = CreateSummaryPurchaseOrder(id: 7, supplierId: 3, supplierName: "Acme Supplies");

        var pagedResult = new PagedResult<PurchaseOrder>
        {
            Items = new[] { purchaseOrder },
            Page = 2,
            PageSize = 20,
            TotalCount = 57
        };

        var purchaseOrderRepository = new FakePurchaseOrderRepository(pagedResult: pagedResult);
        var handler = new GetPurchaseOrdersHandler(purchaseOrderRepository);

        var request = new GetPurchaseOrdersRequest
        {
            PageNum = 2,
            PageSize = 20
        };

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var page = result.Value!.PurchaseOrders;
        Assert.Equal(2, page.Page);
        Assert.Equal(20, page.PageSize);
        Assert.Equal(57, page.TotalCount);
        Assert.Single(page.Items);
    }

    // =====================================================================
    // Empty result page
    // =====================================================================

    [Fact]
    public async Task HandleAsync_EmptyResultPage_SucceedsWithEmptyItemsAndPreservedMetadata()
    {
        // Arrange — a valid empty PagedResult (no items, metadata intact).
        var pagedResult = new PagedResult<PurchaseOrder>
        {
            Items = Array.Empty<PurchaseOrder>(),
            Page = 4,
            PageSize = 15,
            TotalCount = 0
        };

        var purchaseOrderRepository = new FakePurchaseOrderRepository(pagedResult: pagedResult);
        var handler = new GetPurchaseOrdersHandler(purchaseOrderRepository);

        var request = new GetPurchaseOrdersRequest
        {
            PageNum = 4,
            PageSize = 15
        };

        // Act
        var result = await handler.HandleAsync(request);

        // Assert — success with an empty page; no failure path is invented.
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);

        var page = result.Value!.PurchaseOrders;
        Assert.Empty(page.Items);
        Assert.Equal(4, page.Page);
        Assert.Equal(15, page.PageSize);
        Assert.Equal(0, page.TotalCount);
    }

    [Fact]
    public async Task HandleAsync_UnconfiguredFake_ReturnsTruthfulEmptyPageForRequestedPaging()
    {
        // Arrange — the shared fake returns a truthful empty page for the requested
        // paging when no result is configured.
        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var handler = new GetPurchaseOrdersHandler(purchaseOrderRepository);

        var request = new GetPurchaseOrdersRequest
        {
            PageNum = 6,
            PageSize = 30
        };

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var page = result.Value!.PurchaseOrders;
        Assert.Empty(page.Items);
        Assert.Equal(6, page.Page);
        Assert.Equal(30, page.PageSize);
        Assert.Equal(0, page.TotalCount);
    }

    // =====================================================================
    // Navigation fixture helper (test-only, private to this file)
    // =====================================================================

    /// <summary>
    /// Builds a Draft aggregate with items via real Domain APIs and attaches the
    /// supplier navigation the handler dereferences (<c>po.Supplier.Name</c>). The
    /// property has a private setter with no public attach API (EF Core populates it),
    /// so the smallest test-only mechanism is reflection — mirroring the
    /// <see cref="EntityIdHelper"/> precedent. No production visibility or setter was
    /// changed.
    /// </summary>
    private static PurchaseOrder CreateSummaryPurchaseOrder(
        int id,
        int supplierId,
        string supplierName,
        params (int ProductId, decimal Quantity, decimal UnitCost)[] items)
    {
        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrder(id: id, supplierId: supplierId);

        foreach (var (productId, quantity, unitCost) in items)
        {
            purchaseOrder.AddItem(productId, quantity, unitCost);
        }

        typeof(PurchaseOrder)
            .GetProperty(nameof(PurchaseOrder.Supplier))!
            .SetValue(purchaseOrder, PurchasingTestData.CreateSupplier(id: supplierId, name: supplierName));

        return purchaseOrder;
    }
}
