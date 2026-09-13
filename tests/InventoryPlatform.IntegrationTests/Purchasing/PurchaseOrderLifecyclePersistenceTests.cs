using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Infrastructure.Persistence.Context;
using InventoryPlatform.Infrastructure.Persistence.Repositories;
using InventoryPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InventoryPlatform.IntegrationTests.Purchasing;

/// <summary>
/// Sprint 14 T06 integration tests for Sprint 14 Purchase Order lifecycle
/// persistence: cancellation, Draft item editing, and item removal round-trip
/// through the real <see cref="PurchaseOrderRepository"/> and the real
/// <see cref="UnitOfWork"/> (the existing EF Core InMemory pattern — no new
/// provider, no new package).
///
/// <para>
/// Fresh-context isolation (mandatory): no persistence assertion runs against
/// the context that performed the mutation. Every test follows the established
/// PurchaseOrderRepositoryTests round-trip shape — arrange + save in one
/// explicit context scope, dispose it, reload through the real repository in a
/// new context, mutate via the real Domain methods, save through the real
/// <see cref="UnitOfWork"/> in that same mutation context, dispose it, and
/// reload in a final fresh context to assert persisted state.
/// </para>
///
/// <para>
/// T06 proves persistence only. Cancellation transition validation, Draft-only
/// enforcement, item-not-found, and quantity/unit-cost invariants are Domain
/// behavior already covered by T02-T04 unit tests and are not re-tested here.
/// </para>
///
/// <para>
/// EF Core InMemory limitation: these tests prove repository wiring, aggregate
/// round-trip behavior, change tracking across fresh contexts, and delete
/// persistence in the configured model. They do NOT prove SQL Server SQL
/// translation, FK/cascade constraint enforcement at the relational level,
/// transaction semantics, or provider-specific behavior.
/// </para>
/// </summary>
public sealed class PurchaseOrderLifecyclePersistenceTests
{
    // Unique per-test-instance database (xUnit creates a new instance per test).
    private readonly string _databaseName;
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public PurchaseOrderLifecyclePersistenceTests()
    {
        _databaseName = $"POLifecycleTest_{Guid.NewGuid():N}";
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: _databaseName)
            .Options;
    }

    // =====================================================================
    // Cancellation persistence
    // =====================================================================

    [Fact]
    public async Task Cancel_DraftPurchaseOrder_PersistsCancelledStatus()
    {
        // Arrange + persist baseline (Draft).
        var supplierId = await SeedSupplier("Cancel Draft Co");
        var productId = await SeedProduct("SKU-CD", "Cancel Draft Widget");

        var purchaseOrder = PurchaseOrder.Create(
            supplierId: supplierId,
            orderDate: new DateOnly(2026, 7, 1),
            expectedDeliveryDate: null,
            remarks: null);
        purchaseOrder.AddItem(productId, quantity: 4m, unitCost: 8.00m);

        await AddAndSave(purchaseOrder);

        // Fresh mutation context — reload through the repository, cancel, save.
        using (var mutationContext = new ApplicationDbContext(_options))
        {
            var repository = new PurchaseOrderRepository(mutationContext);
            var reloaded = await repository.GetByIdAsync(purchaseOrder.Id);

            Assert.NotNull(reloaded);
            Assert.Equal(PurchaseOrderStatus.Draft, reloaded.Status);

            reloaded.Cancel();

            await SaveAsync(mutationContext);
        } // mutation context disposal

        // Final fresh context — assert persisted status.
        using (var verificationContext = new ApplicationDbContext(_options))
        {
            var repository = new PurchaseOrderRepository(verificationContext);
            var persisted = await repository.GetByIdAsync(purchaseOrder.Id);

            Assert.NotNull(persisted);
            Assert.Equal(PurchaseOrderStatus.Cancelled, persisted.Status);
        }
    }

    [Fact]
    public async Task Cancel_SubmittedPurchaseOrder_PersistsCancelledStatus()
    {
        // Arrange + persist baseline (Submitted after real Submit()).
        var supplierId = await SeedSupplier("Cancel Submitted Co");
        var productId = await SeedProduct("SKU-CS", "Cancel Submitted Widget");

        var purchaseOrder = PurchaseOrder.Create(
            supplierId: supplierId,
            orderDate: new DateOnly(2026, 7, 2),
            expectedDeliveryDate: null,
            remarks: null);
        purchaseOrder.AddItem(productId, quantity: 2m, unitCost: 15.00m);
        purchaseOrder.Submit();

        await AddAndSave(purchaseOrder);

        // Fresh mutation context — reload through the repository, cancel, save.
        using (var mutationContext = new ApplicationDbContext(_options))
        {
            var repository = new PurchaseOrderRepository(mutationContext);
            var reloaded = await repository.GetByIdAsync(purchaseOrder.Id);

            Assert.NotNull(reloaded);
            Assert.Equal(PurchaseOrderStatus.Submitted, reloaded.Status);

            reloaded.Cancel();

            await SaveAsync(mutationContext);
        } // mutation context disposal

        // Final fresh context — assert persisted status.
        using (var verificationContext = new ApplicationDbContext(_options))
        {
            var repository = new PurchaseOrderRepository(verificationContext);
            var persisted = await repository.GetByIdAsync(purchaseOrder.Id);

            Assert.NotNull(persisted);
            Assert.Equal(PurchaseOrderStatus.Cancelled, persisted.Status);
        }
    }

    // =====================================================================
    // Item update persistence
    // =====================================================================

    [Fact]
    public async Task UpdateItem_PersistsQuantityAndUnitCost()
    {
        // Arrange + persist baseline: 3 x 10.00.
        var supplierId = await SeedSupplier("Update Item Co");
        var productId = await SeedProduct("SKU-UI", "Update Item Widget");

        var purchaseOrder = PurchaseOrder.Create(
            supplierId: supplierId,
            orderDate: new DateOnly(2026, 7, 3),
            expectedDeliveryDate: null,
            remarks: null);
        purchaseOrder.AddItem(productId, quantity: 3m, unitCost: 10.00m);

        await AddAndSave(purchaseOrder);

        // The original values (3m / 10.00m) differ from the updated values
        // (7.25m / 4.50m); the baseline below is verified from a fresh context
        // before the mutation so the final assertions prove a real change.
        using (var verificationContext = new ApplicationDbContext(_options))
        {
            var repository = new PurchaseOrderRepository(verificationContext);
            var baseline = await repository.GetByIdAsync(purchaseOrder.Id);
            Assert.NotNull(baseline);
            Assert.Equal(3m, baseline.Items.Single().Quantity);
            Assert.Equal(10.00m, baseline.Items.Single().UnitCost);
        }

        // Fresh mutation context — reload, update quantity AND unit cost, save.
        using (var mutationContext = new ApplicationDbContext(_options))
        {
            var repository = new PurchaseOrderRepository(mutationContext);
            var reloaded = await repository.GetByIdAsync(purchaseOrder.Id);

            Assert.NotNull(reloaded);

            reloaded.UpdateItem(productId, quantity: 7.25m, unitCost: 4.50m);

            await SaveAsync(mutationContext);
        } // mutation context disposal

        // Final fresh context — both values explicitly proven after reload.
        using (var finalContext = new ApplicationDbContext(_options))
        {
            var repository = new PurchaseOrderRepository(finalContext);
            var persisted = await repository.GetByIdAsync(purchaseOrder.Id);

            Assert.NotNull(persisted);
            var item = Assert.Single(persisted.Items);
            Assert.Equal(productId, item.ProductId);
            Assert.Equal(7.25m, item.Quantity);
            Assert.Equal(4.50m, item.UnitCost);
        }
    }

    // =====================================================================
    // Item removal persistence (mandatory, high priority)
    // =====================================================================

    [Fact]
    public async Task RemoveItem_PersistsChildDeletion()
    {
        // Arrange + persist baseline: Draft with exactly two items.
        var supplierId = await SeedSupplier("Remove Item Co");
        var keepProductId = await SeedProduct("SKU-RK", "Kept Widget");
        var removeProductId = await SeedProduct("SKU-RM", "Removed Widget");

        var purchaseOrder = PurchaseOrder.Create(
            supplierId: supplierId,
            orderDate: new DateOnly(2026, 7, 4),
            expectedDeliveryDate: null,
            remarks: null);
        purchaseOrder.AddItem(keepProductId, quantity: 6m, unitCost: 5.00m);
        purchaseOrder.AddItem(removeProductId, quantity: 9m, unitCost: 2.00m);

        await AddAndSave(purchaseOrder);

        // Fresh mutation context — reload through the repository, remove one
        // item by ProductId, save.
        using (var mutationContext = new ApplicationDbContext(_options))
        {
            var repository = new PurchaseOrderRepository(mutationContext);
            var reloaded = await repository.GetByIdAsync(purchaseOrder.Id);

            Assert.NotNull(reloaded);
            Assert.Equal(2, reloaded.Items.Count);

            reloaded.RemoveItem(removeProductId);

            await SaveAsync(mutationContext);
        } // mutation context disposal

        // Final fresh context — removed child absent, remaining child intact.
        using (var finalContext = new ApplicationDbContext(_options))
        {
            var repository = new PurchaseOrderRepository(finalContext);
            var persisted = await repository.GetByIdAsync(purchaseOrder.Id);

            Assert.NotNull(persisted);
            Assert.Equal(PurchaseOrderStatus.Draft, persisted.Status);
            Assert.Single(persisted.Items);
            Assert.Contains(persisted.Items, i => i.ProductId == keepProductId);
            Assert.DoesNotContain(persisted.Items, i => i.ProductId == removeProductId);

            // Direct child-set verification through the DbContext: the deleted
            // child row no longer exists for this Purchase Order.
            var childRows = await finalContext.PurchaseOrderItems
                .AsNoTracking()
                .Where(i => i.PurchaseOrderId == purchaseOrder.Id)
                .ToListAsync();
            Assert.Single(childRows);
            Assert.DoesNotContain(childRows, i => i.ProductId == removeProductId);
        }
    }

    // =====================================================================
    // Final-item removal persistence
    // =====================================================================

    [Fact]
    public async Task RemoveItem_FinalItem_PersistsEmptyDraft()
    {
        // Arrange + persist baseline: Draft with exactly one item.
        var supplierId = await SeedSupplier("Final Item Co");
        var productId = await SeedProduct("SKU-FI", "Final Item Widget");

        var purchaseOrder = PurchaseOrder.Create(
            supplierId: supplierId,
            orderDate: new DateOnly(2026, 7, 5),
            expectedDeliveryDate: null,
            remarks: null);
        purchaseOrder.AddItem(productId, quantity: 1m, unitCost: 3.00m);

        await AddAndSave(purchaseOrder);

        // Fresh mutation context — reload, remove the final item, save.
        using (var mutationContext = new ApplicationDbContext(_options))
        {
            var repository = new PurchaseOrderRepository(mutationContext);
            var reloaded = await repository.GetByIdAsync(purchaseOrder.Id);

            Assert.NotNull(reloaded);
            Assert.Single(reloaded.Items);

            reloaded.RemoveItem(productId);

            Assert.Empty(reloaded.Items); // Domain contract: empty Draft allowed

            await SaveAsync(mutationContext);
        } // mutation context disposal

        // Final fresh context — PO exists, status Draft, zero items persisted.
        using (var finalContext = new ApplicationDbContext(_options))
        {
            var repository = new PurchaseOrderRepository(finalContext);
            var persisted = await repository.GetByIdAsync(purchaseOrder.Id);

            Assert.NotNull(persisted);
            Assert.Equal(PurchaseOrderStatus.Draft, persisted.Status);
            Assert.Empty(persisted.Items);

            // Direct child-set verification: zero child rows remain.
            var childRowCount = await finalContext.PurchaseOrderItems
                .AsNoTracking()
                .CountAsync(i => i.PurchaseOrderId == purchaseOrder.Id);
            Assert.Equal(0, childRowCount);
        }
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    /// <summary>
    /// Adds and saves an aggregate through the real repository and the real
    /// <see cref="UnitOfWork"/>, then disposes the context scope.
    /// </summary>
    private async Task AddAndSave(PurchaseOrder purchaseOrder)
    {
        using (var context = new ApplicationDbContext(_options))
        {
            var repository = new PurchaseOrderRepository(context);
            await repository.AddAsync(purchaseOrder);

            var unitOfWork = new UnitOfWork(context);
            await unitOfWork.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Saves through the real <see cref="UnitOfWork"/> — the production
    /// persistence boundary used by the T03/T04 handlers.
    /// </summary>
    private static async Task SaveAsync(ApplicationDbContext context)
    {
        var unitOfWork = new UnitOfWork(context);
        await unitOfWork.SaveChangesAsync();
    }

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
    /// Adds and saves the Category/Unit/Product chain a Product requires
    /// (matching the existing SeedProduct convention); returns the product's
    /// persistence-assigned id.
    /// </summary>
    private async Task<int> SeedProduct(string sku, string name)
    {
        using var context = new ApplicationDbContext(_options);

        // Category and Unit must be saved before the Product is constructed: the
        // Product ctor guard-rejects zero/negative category/unit ids, and
        // persistence-assigned ids only exist after SaveChanges.
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
}
