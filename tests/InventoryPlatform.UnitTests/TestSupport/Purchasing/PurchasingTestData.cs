using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;

namespace InventoryPlatform.UnitTests.TestSupport.Purchasing;

/// <summary>
/// Purchasing-specific test-data helpers constructing valid Domain objects through the
/// real Domain APIs, shared by the Sprint 13 Purchasing test tasks (T02-T05) per the
/// accepted Rule-of-Three disposition in plan/SPRINT_13_PLANNING_REPORT.md Section 12.1.
///
/// Scope is deliberately small: only the reusable shapes the accepted downstream tasks
/// require. No parallel object model, no configurable builder framework.
///
/// Conventions (matching the existing Domain test suites):
/// - fixed, deterministic default dates (no DateTime.Now/UtcNow in defaults);
/// - invalid-state variants are produced by mutating a valid object through real Domain
///   methods (e.g., <c>Deactivate()</c>), never by bypassing Domain rules.
/// </summary>
public static class PurchasingTestData
{
    // Deterministic dates shared by all Purchasing test fixtures (T02-T05).
    public static readonly DateOnly DefaultOrderDate = new(2026, 1, 1);
    public static readonly DateOnly DefaultExpectedDeliveryDate = new(2026, 2, 1);

    // --- Suppliers ---

    /// <summary>Creates a valid, active supplier using the real Supplier constructor.</summary>
    public static Supplier CreateSupplier(
        string name = "Test Supplier",
        string? contactPerson = null,
        string? email = null,
        string? phone = null,
        string? address = null)
        => new(name, contactPerson, email, phone, address);

    /// <summary>Creates a valid supplier with the given id assigned via reflection.</summary>
    public static Supplier CreateSupplier(int id, string name = "Test Supplier")
    {
        var supplier = CreateSupplier(name);
        EntityIdHelper.SetEntityId(supplier, id);

        return supplier;
    }

    /// <summary>Creates a valid supplier and deactivates it through the real Domain API.</summary>
    public static Supplier CreateInactiveSupplier(string name = "Test Supplier")
    {
        var supplier = CreateSupplier(name);
        supplier.Deactivate();

        return supplier;
    }

    // --- Products ---

    /// <summary>Creates a valid, active product using the real Product constructor.</summary>
    public static Product CreateProduct(
        string sku = "SKU-0001",
        string name = "Test Product",
        int categoryId = 1,
        int unitId = 1,
        decimal quantityOnHand = 0m,
        decimal costPrice = 0m,
        decimal sellingPrice = 0m)
        => new(sku, name, categoryId, unitId, quantityOnHand, costPrice, sellingPrice);

    /// <summary>Creates a valid product with the given id assigned via reflection.</summary>
    public static Product CreateProduct(int id, string sku = "SKU-0001", string name = "Test Product")
    {
        var product = CreateProduct(sku, name);
        EntityIdHelper.SetEntityId(product, id);

        return product;
    }

    /// <summary>Creates a valid product and deactivates it through the real Domain API.</summary>
    public static Product CreateInactiveProduct(string sku = "SKU-0001", string name = "Test Product")
    {
        var product = CreateProduct(sku, name);
        product.Deactivate();

        return product;
    }

    // --- Purchase orders ---

    /// <summary>
    /// Creates a valid Draft purchase order via the real <see cref="PurchaseOrder.Create"/>
    /// factory. Items must be added through <see cref="PurchaseOrder.AddItem"/> (or the
    /// <c>WithItems</c> helpers below); <see cref="PurchaseOrderItem"/> has no public ctor.
    /// </summary>
    public static PurchaseOrder CreateDraftPurchaseOrder(
        int supplierId = 1,
        DateOnly? orderDate = null,
        DateOnly? expectedDeliveryDate = null,
        string? remarks = null)
        => PurchaseOrder.Create(
            supplierId,
            orderDate ?? DefaultOrderDate,
            expectedDeliveryDate,
            remarks);

    /// <summary>
    /// Creates a valid Draft purchase order with the given id assigned via reflection.
    /// The id is required whenever a test must correlate the aggregate with handler
    /// requests or repository calls.
    /// </summary>
    public static PurchaseOrder CreateDraftPurchaseOrder(
        int id,
        int supplierId,
        DateOnly? orderDate = null,
        DateOnly? expectedDeliveryDate = null,
        string? remarks = null)
    {
        var order = CreateDraftPurchaseOrder(supplierId, orderDate, expectedDeliveryDate, remarks);
        EntityIdHelper.SetEntityId(order, id);

        return order;
    }

    /// <summary>
    /// Creates a valid Draft purchase order containing one item for the given product
    /// (via <see cref="PurchaseOrder.AddItem"/>).
    /// </summary>
    public static PurchaseOrder CreateDraftPurchaseOrderWithItem(
        int productId = 10,
        decimal quantity = 5m,
        decimal unitCost = 25.00m,
        int supplierId = 1,
        DateOnly? orderDate = null,
        DateOnly? expectedDeliveryDate = null,
        string? remarks = null)
    {
        var order = CreateDraftPurchaseOrder(supplierId, orderDate, expectedDeliveryDate, remarks);
        order.AddItem(productId, quantity, unitCost);

        return order;
    }

    /// <summary>
    /// Creates a valid Draft purchase order with the given id containing one item for the
    /// given product (via <see cref="PurchaseOrder.AddItem"/>).
    /// </summary>
    public static PurchaseOrder CreateDraftPurchaseOrderWithItem(
        int id,
        int supplierId,
        int productId = 10,
        decimal quantity = 5m,
        decimal unitCost = 25.00m,
        DateOnly? orderDate = null,
        DateOnly? expectedDeliveryDate = null,
        string? remarks = null)
    {
        var order = CreateDraftPurchaseOrder(id, supplierId, orderDate, expectedDeliveryDate, remarks);
        order.AddItem(productId, quantity, unitCost);

        return order;
    }

    // --- Transition-ready purchase orders ---

    /// <summary>
    /// Creates a valid Submitted purchase order (Draft with at least one item, then
    /// <see cref="PurchaseOrder.Submit"/>) - the entry state for Approve tests (T03).
    /// </summary>
    public static PurchaseOrder CreateSubmittedPurchaseOrder(
        int id,
        int supplierId = 1,
        int productId = 10,
        decimal quantity = 5m,
        decimal unitCost = 25.00m)
    {
        var order = CreateDraftPurchaseOrderWithItem(id, supplierId, productId, quantity, unitCost);
        order.Submit();

        return order;
    }

    /// <summary>
    /// Creates a valid Approved purchase order (Submitted, then
    /// <see cref="PurchaseOrder.Approve"/>) - the entry state for Receive tests (T04).
    /// </summary>
    public static PurchaseOrder CreateApprovedPurchaseOrder(
        int id,
        int supplierId = 1,
        int productId = 10,
        decimal quantity = 5m,
        decimal unitCost = 25.00m)
    {
        var order = CreateSubmittedPurchaseOrder(id, supplierId, productId, quantity, unitCost);
        order.Approve();

        return order;
    }
}
