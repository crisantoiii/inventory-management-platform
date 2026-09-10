using InventoryPlatform.Application.Features.Purchasing;
using InventoryPlatform.Application.Features.Purchasing.UpdatePurchaseOrderItem;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Domain.Exceptions;
using InventoryPlatform.UnitTests.TestSupport.Purchasing;
using Xunit;

namespace InventoryPlatform.UnitTests.Application.Purchasing;

public sealed class UpdatePurchaseOrderItemHandlerTests
{
    // =====================================================================
    // Success
    // =====================================================================

    [Fact]
    public async Task HandleAsync_DraftPurchaseOrder_UpdatesItemAndReturnsUpdatedResponse()
    {
        // Arrange
        const int purchaseOrderId = 7;
        const int productId = 10;
        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrderWithItem(
            id: purchaseOrderId,
            supplierId: 1,
            productId: productId,
            quantity: 5m,
            unitCost: 25.00m);

        var request = new UpdatePurchaseOrderItemRequest(
            PurchaseOrderId: purchaseOrderId,
            ProductId: productId,
            Quantity: 12m,
            UnitCost: 30.50m);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new UpdatePurchaseOrderItemHandler(purchaseOrderRepository, unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);

        var response = result.Value!;
        Assert.Equal(purchaseOrderId, response.PurchaseOrderId);
        Assert.Equal(productId, response.ProductId);
        Assert.Equal(12m, response.Quantity);
        Assert.Equal(30.50m, response.UnitCost);

        // The loaded (tracked) aggregate item itself was mutated by the Domain.
        var item = Assert.Single(purchaseOrder.Items);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal(12m, item.Quantity);
        Assert.Equal(30.50m, item.UnitCost);

        // Aggregate total reflects the Domain recalculation.
        Assert.Equal(366m, purchaseOrder.TotalAmount);

        Assert.Equal(1, purchaseOrderRepository.GetByIdAsyncCallCount);
        Assert.Equal(purchaseOrderId, purchaseOrderRepository.LastGetByIdAsyncRequestId);
        Assert.Equal(1, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_DraftPurchaseOrder_SaveOccursAfterSuccessfulUpdate()
    {
        // Arrange
        var callOrder = new CallOrder();
        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrderWithItem(
            id: 7,
            supplierId: 1);

        var request = new UpdatePurchaseOrderItemRequest(
            PurchaseOrderId: 7,
            ProductId: 10,
            Quantity: 8m,
            UnitCost: 20.00m);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(
            purchaseOrder: purchaseOrder,
            callOrder: callOrder);
        var unitOfWork = new FakeUnitOfWork(callOrder: callOrder);

        var handler = new UpdatePurchaseOrderItemHandler(purchaseOrderRepository, unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, unitOfWork.SaveChangesAsyncCallCount);
        Assert.Equal(
            new[]
            {
                "PurchaseOrderRepository.GetByIdAsync",
                "UnitOfWork.SaveChangesAsync"
            },
            callOrder.Events);
    }

    // =====================================================================
    // Purchase order not found
    // =====================================================================

    [Fact]
    public async Task HandleAsync_PurchaseOrderNotFound_ReturnsNotFoundFailureAndDoesNotSave()
    {
        // Arrange
        var request = new UpdatePurchaseOrderItemRequest(
            PurchaseOrderId: 42,
            ProductId: 10,
            Quantity: 8m,
            UnitCost: 20.00m);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder: null);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new UpdatePurchaseOrderItemHandler(purchaseOrderRepository, unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(PurchaseOrderErrors.NotFound, result.Error);
        Assert.Equal("PurchaseOrder.NotFound", result.Error.Code);
        Assert.Equal("Purchase order not found.", result.Error.Message);

        Assert.Equal(1, purchaseOrderRepository.GetByIdAsyncCallCount);
        Assert.Equal(42, purchaseOrderRepository.LastGetByIdAsyncRequestId);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // Item not found (Domain contract propagation)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_UnknownProductId_PropagatesDomainExceptionAndDoesNotSave()
    {
        // Arrange
        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrderWithItem(
            id: 7,
            supplierId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        var request = new UpdatePurchaseOrderItemRequest(
            PurchaseOrderId: 7,
            ProductId: 99,
            Quantity: 8m,
            UnitCost: 20.00m);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new UpdatePurchaseOrderItemHandler(purchaseOrderRepository, unitOfWork);

        // Act
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(request));

        // Assert
        Assert.Equal("Purchase order item was not found.", exception.Message);

        var item = Assert.Single(purchaseOrder.Items);
        Assert.Equal(5m, item.Quantity);
        Assert.Equal(25.00m, item.UnitCost);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // Invalid state: Submitted order (Domain Draft-only guard)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_SubmittedPurchaseOrder_PropagatesDomainExceptionAndDoesNotSave()
    {
        // Arrange
        var purchaseOrder = PurchasingTestData.CreateSubmittedPurchaseOrder(
            id: 7,
            supplierId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        var request = new UpdatePurchaseOrderItemRequest(
            PurchaseOrderId: 7,
            ProductId: 10,
            Quantity: 8m,
            UnitCost: 20.00m);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new UpdatePurchaseOrderItemHandler(purchaseOrderRepository, unitOfWork);

        // Act
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(request));

        // Assert
        Assert.Equal("Only draft purchase orders can be modified.", exception.Message);
        Assert.Equal(PurchaseOrderStatus.Submitted, purchaseOrder.Status);

        var item = Assert.Single(purchaseOrder.Items);
        Assert.Equal(5m, item.Quantity);
        Assert.Equal(25.00m, item.UnitCost);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // Invalid quantity (Domain rule propagation)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_NonPositiveQuantity_PropagatesDomainExceptionAndDoesNotSave()
    {
        // Arrange
        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrderWithItem(
            id: 7,
            supplierId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        var request = new UpdatePurchaseOrderItemRequest(
            PurchaseOrderId: 7,
            ProductId: 10,
            Quantity: 0m,
            UnitCost: 20.00m);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new UpdatePurchaseOrderItemHandler(purchaseOrderRepository, unitOfWork);

        // Act
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(request));

        // Assert
        Assert.Equal("Quantity must be greater than zero.", exception.Message);

        var item = Assert.Single(purchaseOrder.Items);
        Assert.Equal(5m, item.Quantity);
        Assert.Equal(25.00m, item.UnitCost);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // Invalid unit cost (Domain rule propagation)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_NegativeUnitCost_PropagatesDomainExceptionAndDoesNotSave()
    {
        // Arrange
        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrderWithItem(
            id: 7,
            supplierId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        var request = new UpdatePurchaseOrderItemRequest(
            PurchaseOrderId: 7,
            ProductId: 10,
            Quantity: 8m,
            UnitCost: -1m);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new UpdatePurchaseOrderItemHandler(purchaseOrderRepository, unitOfWork);

        // Act
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(request));

        // Assert
        Assert.Equal("Unit cost cannot be negative.", exception.Message);

        var item = Assert.Single(purchaseOrder.Items);
        Assert.Equal(5m, item.Quantity);
        Assert.Equal(25.00m, item.UnitCost);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // Invalid state: Cancelled order (T02 terminal behavior integration)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_CancelledPurchaseOrder_PropagatesDomainExceptionAndDoesNotSave()
    {
        // Arrange
        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrderWithItem(
            id: 7,
            supplierId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);
        purchaseOrder.Cancel();

        var request = new UpdatePurchaseOrderItemRequest(
            PurchaseOrderId: 7,
            ProductId: 10,
            Quantity: 8m,
            UnitCost: 20.00m);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new UpdatePurchaseOrderItemHandler(purchaseOrderRepository, unitOfWork);

        // Act
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(request));

        // Assert
        Assert.Equal("Only draft purchase orders can be modified.", exception.Message);
        Assert.Equal(PurchaseOrderStatus.Cancelled, purchaseOrder.Status);

        var item = Assert.Single(purchaseOrder.Items);
        Assert.Equal(5m, item.Quantity);
        Assert.Equal(25.00m, item.UnitCost);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }
}
