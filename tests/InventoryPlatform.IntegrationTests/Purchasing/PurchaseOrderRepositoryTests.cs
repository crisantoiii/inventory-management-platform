using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Infrastructure.Persistence.Repositories;
using InventoryPlatform.Shared.Paging;
using InventoryPlatform.Shared.Sorting;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryPlatform.IntegrationTests.Purchasing;

/// <summary>
/// T06 integration tests for the real <see cref="PurchaseOrderRepository"/>, using the
/// existing EF Core InMemory pattern of the IntegrationTests project (no new provider,
/// no new package).
///
/// <para>
/// Mandatory fresh-context isolation: navigation-loading and persistence-round-trip
/// tests NEVER arrange and query with the same DbContext. EF Core InMemory performs
/// relationship fix-up from the change tracker, so querying with the arrange context
/// could populate navigation properties even if the repository's Include chain were
/// missing (a false-positive Include assertion). Every such test therefore uses a unique
/// per-test database (a fresh <c>Guid</c> per test instance), saves and disposes the
/// arrange context, then creates a fresh query context against the same database before
/// executing the repository query.
/// </para>
///
/// <para>
/// These tests verify repository wiring and query shape only. EF Core InMemory does NOT
/// prove SQL Server SQL translation, collation behavior, FK/unique constraint
/// enforcement, transaction semantics, provider-specific date/string behavior, or
/// relational performance/query-plan behavior.
/// </para>
///
/// <para>
/// AsNoTracking is verified only where the current source actually applies it:
/// <see cref="PurchaseOrderRepository.GetPagedAsync"/> uses AsNoTracking;
/// <see cref="PurchaseOrderRepository.GetByIdAsync"/> does NOT (it is tracked) — no
/// AsNoTracking claim is made for GetByIdAsync.
/// </para>
/// </summary>
public sealed class PurchaseOrderRepositoryTests
{
    // Unique per-test-instance database (xUnit creates a new instance per test).
    private readonly string _databaseName;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public PurchaseOrderRepositoryTests()
    {
        _databaseName = $"PORepoTest_{Guid.NewGuid():N}";
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;
    }

    // =====================================================================
    // GetByIdAsync — not found
    // =====================================================================

    [Fact]
    public async Task GetByIdAsync_WhenPurchaseOrderDoesNotExist_ReturnsNull()
    {
        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        var result = await repository.GetByIdAsync(9999);

        Assert.Null(result);
    }

    // =====================================================================
    // GetByIdAsync — Includes verified from a fresh query context
    // =====================================================================

    [Fact]
    public async Task GetByIdAsync_WhenPurchaseOrderExists_ReturnsAggregateWithSupplierItemsAndProducts()
    {
        // Arrange — dependencies first so the aggregate can reference persisted ids.
        var supplierId = await SeedSupplier("Acme Supplies");
        var productId = await SeedProduct("SKU-10", "Standard Widget");

        var purchaseOrder = PurchaseOrder.Create(
            supplierId: supplierId,
            orderDate: new DateOnly(2026, 1, 1),
            expectedDeliveryDate: new DateOnly(2026, 2, 1),
            remarks: "Urgent delivery");
        purchaseOrder.AddItem(productId, quantity: 5m, unitCost: 25.00m);

        await SavePurchaseOrder(purchaseOrder);

        // Fresh query context — relationship fix-up from the arrange context can no
        // longer populate navigations; only the repository's Include chain can.
        using var queryContext = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(queryContext);

        var result = await repository.GetByIdAsync(purchaseOrder.Id);

        Assert.NotNull(result);
        Assert.Equal(supplierId, result.SupplierId);
        Assert.Equal("Acme Supplies", result.Supplier.Name);
        Assert.Single(result.Items);

        var item = result.Items.Single();
        Assert.Equal(productId, item.ProductId);
        Assert.NotNull(item.Product);
        Assert.Equal("SKU-10", item.Product.Sku);
        Assert.Equal("Standard Widget", item.Product.Name);
        Assert.Equal(5m, item.Quantity);
        Assert.Equal(25.00m, item.UnitCost);
        Assert.Equal(PurchaseOrderStatus.Draft, result.Status);
        Assert.Equal(new DateOnly(2026, 1, 1), result.OrderDate);
        Assert.Equal(new DateOnly(2026, 2, 1), result.ExpectedDeliveryDate);
        Assert.Equal("Urgent delivery", result.Remarks);
    }

    [Fact]
    public async Task GetByIdAsync_WhenPurchaseOrderExists_IncludesAllItemsAndTheirProducts()
    {
        // Arrange
        var supplierId = await SeedSupplier("Acme Supplies");
        var product1Id = await SeedProduct("SKU-10", "Standard Widget");
        var product2Id = await SeedProduct("SKU-20", "Bulk Widget");

        var purchaseOrder = PurchaseOrder.Create(
            supplierId: supplierId,
            orderDate: new DateOnly(2026, 1, 1),
            expectedDeliveryDate: null,
            remarks: null);
        purchaseOrder.AddItem(product1Id, quantity: 5m, unitCost: 25.00m);
        purchaseOrder.AddItem(product2Id, quantity: 3m, unitCost: 10.00m);

        await SavePurchaseOrder(purchaseOrder);

        using var queryContext = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(queryContext);

        var result = await repository.GetByIdAsync(purchaseOrder.Id);

        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(
            result.Items,
            i => i.ProductId == product1Id && i.Product.Sku == "SKU-10" &&
                 i.Quantity == 5m && i.UnitCost == 25.00m);
        Assert.Contains(
            result.Items,
            i => i.ProductId == product2Id && i.Product.Sku == "SKU-20" &&
                 i.Quantity == 3m && i.UnitCost == 10.00m);
    }

    // =====================================================================
    // GetPagedAsync — search (exactly as implemented)
    // =====================================================================

    [Fact]
    public async Task GetPagedAsync_SearchMatchesSupplierName_ReturnsMatchingOrders()
    {
        await SeedThreeOrders();

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        var query = new PagedQuery { PageNum = 1, PageSize = 10, Search = "Globex" };
        var result = await repository.GetPagedAsync(query);

        Assert.Equal(1, result.TotalCount);
        var order = Assert.Single(result.Items);
        Assert.Equal("Globex GmbH", order.Supplier.Name);
    }

    [Fact]
    public async Task GetPagedAsync_NumericSearch_MatchesOrderId()
    {
        // Arrange — numeric search matches po.Id == parsedId OR Supplier.Name.Contains.
        var targetOrderId = await SeedThreeOrders();

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        var query = new PagedQuery { PageNum = 1, PageSize = 10, Search = targetOrderId.ToString() };
        var result = await repository.GetPagedAsync(query);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(targetOrderId, Assert.Single(result.Items).Id);
    }

    [Fact]
    public async Task GetPagedAsync_NumericSearch_AlsoMatchesSupplierNameContainingDigits()
    {
        // Arrange — proves the OR branch of the numeric search: the name side matches
        // even though no order has the parsed id.
        var supplierId = await SeedSupplier("Num 7 Logistics");
        var productId = await SeedProduct("SKU-10", "Standard Widget");
        var purchaseOrder = PurchaseOrder.Create(supplierId, new DateOnly(2026, 1, 1), null, null);
        purchaseOrder.AddItem(productId, quantity: 1m, unitCost: 1m);
        await SavePurchaseOrder(purchaseOrder);

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        // No order has Id 7, but the supplier name contains "7".
        var query = new PagedQuery { PageNum = 1, PageSize = 10, Search = "7" };
        var result = await repository.GetPagedAsync(query);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(purchaseOrder.Id, Assert.Single(result.Items).Id);
    }

    [Fact]
    public async Task GetPagedAsync_SearchWithNoMatches_ReturnsEmptyPage()
    {
        await SeedThreeOrders();

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        var query = new PagedQuery { PageNum = 1, PageSize = 10, Search = "NoSuchSupplier" };
        var result = await repository.GetPagedAsync(query);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetPagedAsync_WhitespaceSearch_IsIgnored()
    {
        await SeedThreeOrders();

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        var query = new PagedQuery { PageNum = 1, PageSize = 10, Search = "   " };
        var result = await repository.GetPagedAsync(query);

        Assert.Equal(3, result.TotalCount);
    }

    // =====================================================================
    // GetPagedAsync — FromDate / ToDate filters and boundaries
    // =====================================================================

    [Fact]
    public async Task GetPagedAsync_FromDateIsInclusive_IncludesBoundary()
    {
        await SeedThreeOrders();

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        // FromDate 2026-02-01 keeps the boundary order (2026-02-01) and 2026-03-01.
        var query = new PagedQuery { PageNum = 1, PageSize = 10 };
        var result = await repository.GetPagedAsync(query, fromDate: new DateOnly(2026, 2, 1));

        Assert.Equal(2, result.TotalCount);
        Assert.Contains(result.Items, po => po.OrderDate == new DateOnly(2026, 2, 1));
        Assert.Contains(result.Items, po => po.OrderDate == new DateOnly(2026, 3, 1));
    }

    [Fact]
    public async Task GetPagedAsync_ToDateIsInclusive_IncludesBoundary()
    {
        await SeedThreeOrders();

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        // ToDate 2026-02-01 keeps 2026-01-01 and the boundary order (2026-02-01).
        var query = new PagedQuery { PageNum = 1, PageSize = 10 };
        var result = await repository.GetPagedAsync(query, toDate: new DateOnly(2026, 2, 1));

        Assert.Equal(2, result.TotalCount);
        Assert.Contains(result.Items, po => po.OrderDate == new DateOnly(2026, 1, 1));
        Assert.Contains(result.Items, po => po.OrderDate == new DateOnly(2026, 2, 1));
    }

    [Fact]
    public async Task GetPagedAsync_FromAndToDate_CombineIntoRange()
    {
        await SeedThreeOrders();

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        // 2026-01-15..2026-02-28 keeps only the 2026-02-01 order.
        var query = new PagedQuery { PageNum = 1, PageSize = 10 };
        var result = await repository.GetPagedAsync(
            query,
            fromDate: new DateOnly(2026, 1, 15),
            toDate: new DateOnly(2026, 2, 28));

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(new DateOnly(2026, 2, 1), Assert.Single(result.Items).OrderDate);
    }

    // =====================================================================
    // GetPagedAsync — status filter
    // =====================================================================

    [Fact]
    public async Task GetPagedAsync_StatusFilter_ReturnsOnlyMatchingStatus()
    {
        await SeedThreeOrders();

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        var query = new PagedQuery { PageNum = 1, PageSize = 10 };
        var result = await repository.GetPagedAsync(query, status: PurchaseOrderStatus.Approved);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(PurchaseOrderStatus.Approved, Assert.Single(result.Items).Status);
    }

    // =====================================================================
    // GetPagedAsync — sorting
    // =====================================================================

    [Fact]
    public async Task GetPagedAsync_SortByIdAscending_OrdersById()
    {
        await SeedThreeOrders();

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        var query = new PagedQuery { PageNum = 1, PageSize = 10, SortBy = PurchaseOrderSortFields.Id };
        var result = await repository.GetPagedAsync(query);

        var ids = result.Items.Select(po => po.Id).ToList();
        Assert.Equal(ids.OrderBy(x => x).ToList(), ids);
    }

    [Fact]
    public async Task GetPagedAsync_SortByIdDescending_OrdersByIdReversed()
    {
        await SeedThreeOrders();

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        var query = new PagedQuery
        {
            PageNum = 1,
            PageSize = 10,
            SortBy = PurchaseOrderSortFields.Id,
            Descending = true
        };
        var result = await repository.GetPagedAsync(query);

        var ids = result.Items.Select(po => po.Id).ToList();
        Assert.Equal(ids.OrderByDescending(x => x).ToList(), ids);
    }

    [Fact]
    public async Task GetPagedAsync_SortBySupplier_OrdersBySupplierName()
    {
        await SeedThreeOrders();

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        var query = new PagedQuery { PageNum = 1, PageSize = 10, SortBy = PurchaseOrderSortFields.Supplier };
        var result = await repository.GetPagedAsync(query);

        var names = result.Items.Select(po => po.Supplier.Name).ToList();
        Assert.Equal(names.OrderBy(n => n, StringComparer.Ordinal).ToList(), names);
    }

    [Fact]
    public async Task GetPagedAsync_SortByOrderDate_OrdersByDate()
    {
        await SeedThreeOrders();

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        var query = new PagedQuery { PageNum = 1, PageSize = 10, SortBy = PurchaseOrderSortFields.OrderDate };
        var result = await repository.GetPagedAsync(query);

        var dates = result.Items.Select(po => po.OrderDate).ToList();
        Assert.Equal(dates.OrderBy(d => d).ToList(), dates);
    }

    [Fact]
    public async Task GetPagedAsync_SortByStatus_OrdersByStatus()
    {
        await SeedThreeOrders();

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        var query = new PagedQuery { PageNum = 1, PageSize = 10, SortBy = PurchaseOrderSortFields.Status };
        var result = await repository.GetPagedAsync(query);

        var statuses = result.Items.Select(po => po.Status).ToList();
        Assert.Equal(statuses.OrderBy(s => s).ToList(), statuses);
    }

    [Fact]
    public async Task GetPagedAsync_SortByTotalAmount_OrdersBySumOfLineTotals()
    {
        await SeedThreeOrders();

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        var query = new PagedQuery { PageNum = 1, PageSize = 10, SortBy = PurchaseOrderSortFields.TotalAmount };
        var result = await repository.GetPagedAsync(query);

        var totals = result.Items.Select(po => po.TotalAmount).ToList();
        Assert.Equal(totals.OrderBy(t => t).ToList(), totals);
    }

    [Fact]
    public async Task GetPagedAsync_NoSortBy_UsesDefaultOrderDateDescThenIdDesc()
    {
        await SeedThreeOrders();

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        var query = new PagedQuery { PageNum = 1, PageSize = 10 };
        var result = await repository.GetPagedAsync(query);

        // Current-source default: OrderDate desc, then Id desc.
        var expected = result.Items
            .OrderByDescending(po => po.OrderDate)
            .ThenByDescending(po => po.Id)
            .ToList();
        Assert.Equal(expected, result.Items);
        // The seeded dates are distinct, so the primary sort key is decisive.
        Assert.Equal(
            new[] { new DateOnly(2026, 3, 1), new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1) },
            result.Items.Select(po => po.OrderDate).ToArray());
    }

    // =====================================================================
    // GetPagedAsync — paging, metadata, empty/out-of-range pages
    // =====================================================================

    [Fact]
    public async Task GetPagedAsync_PageSlicing_RespectsPageNumAndPageSize()
    {
        await SeedThreeOrders();

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        // Page 1 of 2 (PageSize 2): 2 items.
        var page1 = await repository.GetPagedAsync(new PagedQuery { PageNum = 1, PageSize = 2 });
        Assert.Equal(2, page1.Items.Count);
        Assert.Equal(2, page1.PageSize);
        Assert.Equal(3, page1.TotalCount);

        // Page 2 of 2: the remaining 1 item.
        var page2 = await repository.GetPagedAsync(new PagedQuery { PageNum = 2, PageSize = 2 });
        Assert.Single(page2.Items);
        Assert.Equal(2, page2.Page);

        // Page 3 is out of range: empty items, metadata still from the query.
        var page3 = await repository.GetPagedAsync(new PagedQuery { PageNum = 3, PageSize = 2 });
        Assert.Empty(page3.Items);
        Assert.Equal(3, page3.Page);
        Assert.Equal(3, page3.TotalCount);
    }

    [Fact]
    public async Task GetPagedAsync_Metadata_CopiesPageNumPageSizeTotalCount()
    {
        await SeedThreeOrders();

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        var result = await repository.GetPagedAsync(new PagedQuery { PageNum = 2, PageSize = 2 });

        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Single(result.Items);
    }

    // =====================================================================
    // GetPagedAsync — composed filter + sort + paging
    // =====================================================================

    [Fact]
    public async Task GetPagedAsync_ComposedFilterSortAndPage_AppliesAllTogether()
    {
        await SeedThreeOrders();

        using var context = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(context);

        // FromDate 2026-01-15 keeps the 2026-02-01 and 2026-03-01 orders; sorted by
        // OrderDate ascending, the first page of size 1 is the 2026-02-01 order.
        var query = new PagedQuery
        {
            PageNum = 1,
            PageSize = 1,
            SortBy = PurchaseOrderSortFields.OrderDate
        };
        var result = await repository.GetPagedAsync(query, fromDate: new DateOnly(2026, 1, 15));

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.PageSize);
        var first = Assert.Single(result.Items);
        Assert.Equal(new DateOnly(2026, 2, 1), first.OrderDate);
    }

    // =====================================================================
    // GetPagedAsync — AsNoTracking (fresh query context ChangeTracker)
    // =====================================================================

    [Fact]
    public async Task GetPagedAsync_UsesAsNoTracking_ReturnsDetachedEntities()
    {
        await SeedThreeOrders();

        using var queryContext = new ApplicationDbContext(_options);
        var repository = new PurchaseOrderRepository(queryContext);

        var result = await repository.GetPagedAsync(new PagedQuery { PageNum = 1, PageSize = 10 });

        // The loaded purchase orders must NOT be tracked by the query context
        // (current source applies AsNoTracking in GetPagedAsync only).
        foreach (var po in result.Items)
        {
            Assert.Equal(EntityState.Detached, queryContext.Entry(po).State);
        }

        var firstItem = result.Items.SelectMany(po => po.Items).FirstOrDefault();
        if (firstItem is not null)
        {
            Assert.Equal(EntityState.Detached, queryContext.Entry(firstItem).State);
        }
    }

    // =====================================================================
    // Persistence round-trip (fresh read contexts)
    // =====================================================================

    /// <summary>
    /// Full persistence round-trip including a domain mutation performed on an
    /// aggregate loaded by the real repository:
    /// AddAsync -> Save -> dispose -> fresh GetByIdAsync -> Submit() -> Save ->
    /// dispose -> fresh GetByIdAsync -> assert Submitted.
    ///
    /// The aggregate is persisted in Draft — the Submit() transition happens AFTER the
    /// first fresh reload, so the final assertion proves a repository-loaded aggregate's
    /// state mutation actually survives a subsequent save and another fresh-context
    /// reload. This is not an aggregate that was already in a later status before
    /// insertion, not a reflection-based state write, and not a check against an entity
    /// still tracked by the same context.
    /// </summary>
    [Fact]
    public async Task AddAndSave_ThenFreshReload_SubmitAndSave_ThenFreshReload_PersistsSubmittedState()
    {
        // Arrange + initial persist — the aggregate is Draft at insertion time.
        var supplierId = await SeedSupplier("Round Trip Co");
        var productId = await SeedProduct("SKU-RT", "Round Trip Widget");

        var purchaseOrder = PurchaseOrder.Create(
            supplierId: supplierId,
            orderDate: new DateOnly(2026, 5, 1),
            expectedDeliveryDate: new DateOnly(2026, 6, 1),
            remarks: "Round trip");
        purchaseOrder.AddItem(productId, quantity: 7m, unitCost: 12.50m);

        using (var writeContext = new ApplicationDbContext(_options))
        {
            var repository = new PurchaseOrderRepository(writeContext);

            await repository.AddAsync(purchaseOrder);
            await writeContext.SaveChangesAsync(); // first save
        } // first context disposal

        // Fresh reload #1 — only the repository's Include chain can populate the
        // navigations now that the write context is disposed.
        PurchaseOrder? reloaded;
        using (var firstReadContext = new ApplicationDbContext(_options))
        {
            var readRepository = new PurchaseOrderRepository(firstReadContext);
            reloaded = await readRepository.GetByIdAsync(purchaseOrder.Id);

            Assert.NotNull(reloaded);
            Assert.True(reloaded.Id > 0);
            Assert.Equal(supplierId, reloaded.SupplierId);
            Assert.Equal("Round Trip Co", reloaded.Supplier.Name);
            Assert.Equal(new DateOnly(2026, 5, 1), reloaded.OrderDate);
            Assert.Equal(new DateOnly(2026, 6, 1), reloaded.ExpectedDeliveryDate);
            Assert.Equal("Round trip", reloaded.Remarks);
            Assert.Equal(PurchaseOrderStatus.Draft, reloaded.Status);
            Assert.Single(reloaded.Items);

            var item = reloaded.Items.Single();
            Assert.Equal(productId, item.ProductId);
            Assert.Equal("SKU-RT", item.Product.Sku);
            Assert.Equal("Round Trip Widget", item.Product.Name);
            Assert.Equal(7m, item.Quantity);
            Assert.Equal(12.50m, item.UnitCost);
            Assert.Equal(87.50m, reloaded.TotalAmount);

            // Real domain state mutation on the freshly loaded aggregate.
            reloaded!.Submit();
            Assert.Equal(PurchaseOrderStatus.Submitted, reloaded.Status);

            readRepository.Update(reloaded);
            await firstReadContext.SaveChangesAsync(); // second save
        } // second context disposal

        // Fresh reload #2 — proves the Submit() mutation persisted.
        using (var secondReadContext = new ApplicationDbContext(_options))
        {
            var finalRepository = new PurchaseOrderRepository(secondReadContext);
            var final = await finalRepository.GetByIdAsync(purchaseOrder.Id);

            Assert.NotNull(final);
            Assert.Equal(PurchaseOrderStatus.Submitted, final.Status);
            Assert.Single(final.Items);
            Assert.Equal("Round Trip Co", final.Supplier.Name);
            Assert.Equal("SKU-RT", final.Items.Single().Product.Sku);
        }
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    /// <summary>
    /// Adds and saves one supplier in this test's unique database; returns its
    /// persistence-assigned id.
    /// </summary>
    private async Task<int> SeedSupplier(string name)
    {
        using var context = new ApplicationDbContext(_options);
        var supplier = new Supplier(name, null, null, null, null);
        context.Suppliers.Add(supplier);
        await context.SaveChangesAsync();

        return supplier.Id;
    }

    /// <summary>
    /// Adds and saves the Category/Unit/Product chain a Product requires (both
    /// relationships are DeleteBehavior.Restrict in the EF configuration); returns the
    /// product's persistence-assigned id.
    /// </summary>
    private async Task<int> SeedProduct(string sku, string name)
    {
        using var context = new ApplicationDbContext(_options);

        // Category and Unit must be saved before the Product is constructed: the
        // Product ctor guard-rejects zero/negative category/unit ids, and persistence-
        // assigned ids only exist after SaveChanges.
        var category = new Category("Components", null);
        var unit = new Unit("PCS", "Piece", "pcs");
        context.Categories.Add(category);
        context.Units.Add(unit);
        await context.SaveChangesAsync();

        var product = new Product(sku, name, category.Id, unit.Id, 0m, 10m, 25m);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        return product.Id;
    }

    /// <summary>
    /// Adds and saves a purchase order aggregate in this test's unique database.
    /// </summary>
    private async Task SavePurchaseOrder(PurchaseOrder purchaseOrder)
    {
        using var context = new ApplicationDbContext(_options);
        context.PurchaseOrders.Add(purchaseOrder);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds three orders in this test's unique database with distinct suppliers,
    /// dates, statuses, and totals so search/filter/sort assertions stay unambiguous:
    /// <list type="bullet">
    /// <item>order 1 — Acme Supplies, 2026-01-01, Draft, total 125.00 (5x25)</item>
    /// <item>order 2 — Acme Supplies, 2026-02-01, Approved, total 155.00 (5x25 + 3x10)</item>
    /// <item>order 3 — Globex GmbH, 2026-03-01, Submitted, total 30.00 (3x10)</item>
    /// </list>
    /// Returns the persistence-assigned id of order 2.
    /// </summary>
    private async Task<int> SeedThreeOrders()
    {
        var acmeId = await SeedSupplier("Acme Supplies");
        var globexId = await SeedSupplier("Globex GmbH");
        var product10Id = await SeedProduct("SKU-10", "Standard Widget");
        var product20Id = await SeedProduct("SKU-20", "Bulk Widget");

        var order1 = PurchaseOrder.Create(acmeId, new DateOnly(2026, 1, 1), null, null);   // Draft
        order1.AddItem(product10Id, quantity: 5m, unitCost: 25.00m);                        // 125.00

        var order2 = PurchaseOrder.Create(acmeId, new DateOnly(2026, 2, 1), null, null);   // -> Approved
        order2.AddItem(product10Id, quantity: 5m, unitCost: 25.00m);                        // 125.00
        order2.AddItem(product20Id, quantity: 3m, unitCost: 10.00m);                        // 30.00
        order2.Submit();
        order2.Approve();

        var order3 = PurchaseOrder.Create(globexId, new DateOnly(2026, 3, 1), null, null); // -> Submitted
        order3.AddItem(product20Id, quantity: 3m, unitCost: 10.00m);                        // 30.00
        order3.Submit();

        using var context = new ApplicationDbContext(_options);
        context.PurchaseOrders.AddRange(order1, order2, order3);
        await context.SaveChangesAsync();

        return order2.Id;
    }
}
