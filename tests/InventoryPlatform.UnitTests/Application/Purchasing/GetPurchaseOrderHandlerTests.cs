using InventoryPlatform.Application.Features.Purchasing;
using InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrder;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.UnitTests.TestSupport.Purchasing;
using Xunit;

namespace InventoryPlatform.UnitTests.Application.Purchasing;

/// <summary>
/// T05 unit tests for <see cref="GetPurchaseOrderHandler"/>.
///
/// Scope: Application-handler orchestration and response mapping against the shared T01
/// <see cref="FakePurchaseOrderRepository"/>. These tests do NOT verify real EF Core
/// Includes, tracking, SQL translation, or persistence — the fake returns prepared
/// aggregates, and the fixtures populate the Supplier/Product navigations the handler
/// dereferences (<c>po.Supplier.Name</c>, <c>item.Product.Sku</c>,
/// <c>item.Product.Name</c>). Real repository Include-chain verification belongs to T06
/// (PurchaseOrderRepository integration tests).
///
/// Navigation fixtures: <see cref="PurchaseOrder.Supplier"/> and
/// <see cref="PurchaseOrderItem.Product"/> expose private setters with no public attach
/// API (EF Core populates them when loading the aggregate). Per the accepted Navigation
/// Fixture Rules, the smallest test-only mechanism is used: reflection helpers private to
/// this file (mirroring the <see cref="EntityIdHelper"/> precedent). No production
/// visibility or setter was changed for test convenience.
/// </summary>
public sealed class GetPurchaseOrderHandlerTests
{
    // =====================================================================
    // Purchase order not found
    // =====================================================================

    [Fact]
    public async Task HandleAsync_PurchaseOrderNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var request = new GetPurchaseOrderRequest(Id: 42);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder: null);
        var handler = new GetPurchaseOrderHandler(purchaseOrderRepository);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(PurchaseOrderErrors.NotFound, result.Error);
        Assert.Equal("PurchaseOrder.NotFound", result.Error.Code);
        Assert.Equal("Purchase order not found.", result.Error.Message);

        // No fabricated response.
        Assert.Null(result.Value);

        Assert.Equal(1, purchaseOrderRepository.GetByIdAsyncCallCount);
        Assert.Equal(42, purchaseOrderRepository.LastGetByIdAsyncRequestId);
    }

    // =====================================================================
    // Successful detail mapping (header fields)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_PurchaseOrderFound_MapsDetailHeaderFields()
    {
        // Arrange
        var orderDate = new DateOnly(2026, 3, 15);
        var expectedDeliveryDate = new DateOnly(2026, 4, 1);

        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrder(
            id: 7,
            supplierId: 3,
            orderDate: orderDate,
            expectedDeliveryDate: expectedDeliveryDate,
            remarks: "Urgent delivery");
        AttachSupplier(purchaseOrder, PurchasingTestData.CreateSupplier(id: 3, name: "Acme Supplies"));

        // One item so TotalAmount is unambiguous: 5 * 25 = 125. The item's Product
        // navigation is attached because the handler dereferences it for every item.
        purchaseOrder.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        AttachProduct(purchaseOrder.Items.Single(), PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10", name: "Standard Widget"));

        var request = new GetPurchaseOrderRequest(Id: 7);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var handler = new GetPurchaseOrderHandler(purchaseOrderRepository);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var response = result.Value!;
        Assert.Equal(7, response.Id);
        Assert.Equal(3, response.SupplierId);
        Assert.Equal("Acme Supplies", response.SupplierName);
        Assert.Equal(orderDate, response.OrderDate);
        Assert.Equal(expectedDeliveryDate, response.ExpectedDeliveryDate);
        Assert.Equal(PurchaseOrderStatus.Draft, response.Status);
        Assert.Equal("Urgent delivery", response.Remarks);
        Assert.Equal(125.00m, response.TotalAmount);

        Assert.Single(response.Items);

        Assert.Equal(1, purchaseOrderRepository.GetByIdAsyncCallCount);
        Assert.Equal(7, purchaseOrderRepository.LastGetByIdAsyncRequestId);
    }

    [Fact]
    public async Task HandleAsync_PurchaseOrderWithoutOptionalFields_MapsNullExpectedDeliveryAndRemarks()
    {
        // Arrange — the handler dereferences Supplier.Name even when the optional
        // header fields are absent, so the Supplier navigation is still populated.
        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrder(
            id: 7,
            supplierId: 3,
            expectedDeliveryDate: null,
            remarks: null);
        AttachSupplier(purchaseOrder, PurchasingTestData.CreateSupplier(id: 3, name: "Acme Supplies"));

        purchaseOrder.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        AttachProduct(purchaseOrder.Items.Single(), PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10", name: "Standard Widget"));

        var request = new GetPurchaseOrderRequest(Id: 7);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var handler = new GetPurchaseOrderHandler(purchaseOrderRepository);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var response = result.Value!;
        Assert.Null(response.ExpectedDeliveryDate);
        Assert.Null(response.Remarks);
    }

    // =====================================================================
    // Successful detail mapping (item fields, navigation-derived values)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_PurchaseOrderFound_MapsItemFieldsIncludingProductNavigation()
    {
        // Arrange
        var product = PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10", name: "Standard Widget");

        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrder(id: 7, supplierId: 3);
        AttachSupplier(purchaseOrder, PurchasingTestData.CreateSupplier(id: 3, name: "Acme Supplies"));

        purchaseOrder.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);
        AttachProduct(purchaseOrder.Items.Single(), product);

        var request = new GetPurchaseOrderRequest(Id: 7);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var handler = new GetPurchaseOrderHandler(purchaseOrderRepository);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var item = result.Value!.Items.Single();
        Assert.Equal(10, item.ProductId);
        Assert.Equal("SKU-10", item.ProductSku);
        Assert.Equal("Standard Widget", item.ProductName);
        Assert.Equal(5m, item.Quantity);
        Assert.Equal(25.00m, item.UnitCost);
        Assert.Equal(125.00m, item.LineTotal);
        Assert.Equal(0m, item.ReceivedQuantity);
        Assert.Equal(5m, item.RemainingQuantity);
        Assert.False(item.IsFullyReceived);
    }

    [Fact]
    public async Task HandleAsync_MultipleItems_MapsAllItemsAndTotalAmount()
    {
        // Arrange
        var product10 = PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10", name: "Standard Widget");
        var product20 = PurchasingTestData.CreateProduct(id: 20, sku: "SKU-20", name: "Bulk Widget");

        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrder(id: 7, supplierId: 3);
        AttachSupplier(purchaseOrder, PurchasingTestData.CreateSupplier(id: 3, name: "Acme Supplies"));

        purchaseOrder.AddItem(productId: 10, quantity: 5m, unitCost: 25.00m);  // LineTotal 125.00
        purchaseOrder.AddItem(productId: 20, quantity: 3m, unitCost: 10.00m);  // LineTotal 30.00
        AttachProduct(purchaseOrder.Items.Single(i => i.ProductId == 10), product10);
        AttachProduct(purchaseOrder.Items.Single(i => i.ProductId == 20), product20);

        var request = new GetPurchaseOrderRequest(Id: 7);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var handler = new GetPurchaseOrderHandler(purchaseOrderRepository);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var response = result.Value!;
        Assert.Equal(155.00m, response.TotalAmount);

        Assert.Equal(2, response.Items.Count);

        var first = response.Items.ElementAt(0);
        Assert.Equal(10, first.ProductId);
        Assert.Equal("SKU-10", first.ProductSku);
        Assert.Equal("Standard Widget", first.ProductName);
        Assert.Equal(125.00m, first.LineTotal);

        var second = response.Items.ElementAt(1);
        Assert.Equal(20, second.ProductId);
        Assert.Equal("SKU-20", second.ProductSku);
        Assert.Equal("Bulk Widget", second.ProductName);
        Assert.Equal(30.00m, second.LineTotal);
    }

    // =====================================================================
    // Partial receive state mapping (real Domain transitions)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_PartiallyReceivedItem_MapsReceiveStateAndReceivingStatus()
    {
        // Arrange — real Domain transitions: Draft + item -> Submit() -> Approve() -> Receive(4 of 10).
        var product = PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10", name: "Standard Widget");

        var purchaseOrder = PurchasingTestData.CreateApprovedPurchaseOrder(
            id: 7,
            supplierId: 3,
            productId: 10,
            quantity: 10m,
            unitCost: 25.00m);
        purchaseOrder.Receive(productId: 10, quantity: 4m);

        AttachSupplier(purchaseOrder, PurchasingTestData.CreateSupplier(id: 3, name: "Acme Supplies"));
        AttachProduct(purchaseOrder.Items.Single(), product);

        var request = new GetPurchaseOrderRequest(Id: 7);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var handler = new GetPurchaseOrderHandler(purchaseOrderRepository);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var response = result.Value!;
        Assert.Equal(PurchaseOrderStatus.Receiving, response.Status);

        // Receiving does not change the line amount: 10 * 25 = 250.
        Assert.Equal(250.00m, response.TotalAmount);

        var item = response.Items.Single();
        Assert.Equal(4m, item.ReceivedQuantity);
        Assert.Equal(6m, item.RemainingQuantity);
        Assert.False(item.IsFullyReceived);
    }

    // =====================================================================
    // Fully received state mapping (real Domain transitions)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_FullyReceivedItem_MapsCompletedStatusAndFullyReceivedFlags()
    {
        // Arrange — real Domain transitions: Draft + item -> Submit() -> Approve() -> Receive(all 10).
        var product = PurchasingTestData.CreateProduct(id: 10, sku: "SKU-10", name: "Standard Widget");

        var purchaseOrder = PurchasingTestData.CreateApprovedPurchaseOrder(
            id: 7,
            supplierId: 3,
            productId: 10,
            quantity: 10m,
            unitCost: 25.00m);
        purchaseOrder.Receive(productId: 10, quantity: 10m);

        AttachSupplier(purchaseOrder, PurchasingTestData.CreateSupplier(id: 3, name: "Acme Supplies"));
        AttachProduct(purchaseOrder.Items.Single(), product);

        var request = new GetPurchaseOrderRequest(Id: 7);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var handler = new GetPurchaseOrderHandler(purchaseOrderRepository);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var response = result.Value!;
        Assert.Equal(PurchaseOrderStatus.Completed, response.Status);

        var item = response.Items.Single();
        Assert.Equal(10m, item.ReceivedQuantity);
        Assert.Equal(0m, item.RemainingQuantity);
        Assert.True(item.IsFullyReceived);
    }

    // =====================================================================
    // Navigation fixture helpers (test-only, private to this file)
    // =====================================================================

    /// <summary>
    /// Attaches the supplier navigation the handler dereferences
    /// (<c>purchaseOrder.Supplier.Name</c>). The property has a private setter with no
    /// public attach API (EF Core populates it), so the smallest test-only mechanism is
    /// reflection — mirroring the <see cref="EntityIdHelper"/> precedent. No production
    /// visibility or setter was changed.
    /// </summary>
    private static void AttachSupplier(PurchaseOrder purchaseOrder, Supplier supplier) =>
        typeof(PurchaseOrder)
            .GetProperty(nameof(PurchaseOrder.Supplier))!
            .SetValue(purchaseOrder, supplier);

    /// <summary>
    /// Attaches the product navigation the handler dereferences
    /// (<c>item.Product.Sku</c>, <c>item.Product.Name</c>). See <see cref="AttachSupplier"/>
    /// for the mechanism justification.
    /// </summary>
    private static void AttachProduct(PurchaseOrderItem item, Product product) =>
        typeof(PurchaseOrderItem)
            .GetProperty(nameof(PurchaseOrderItem.Product))!
            .SetValue(item, product);
}
