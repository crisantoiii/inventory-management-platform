using InventoryPlatform.Application.Features.Purchasing;
using InventoryPlatform.Application.Features.Purchasing.RemovePurchaseOrderItem;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Domain.Exceptions;
using InventoryPlatform.UnitTests.TestSupport.Purchasing;
using Xunit;

namespace InventoryPlatform.UnitTests.Application.Purchasing;

public sealed class RemovePurchaseOrderItemHandlerTests
{
    // =====================================================================
    // Success
    // =====================================================================

    [Fact]
    public async Task HandleAsync_DraftPurchaseOrder_RemovesItemAndReturnsRemovedResponse()
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

        var request = new RemovePurchaseOrderItemRequest(
            PurchaseOrderId: purchaseOrderId,
            ProductId: productId);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new RemovePurchaseOrderItemHandler(purchaseOrderRepository, unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);

        var response = result.Value!;
        Assert.Equal(purchaseOrderId, response.PurchaseOrderId);
        Assert.Equal(productId, response.ProductId);

        // The loaded (tracked) aggregate itself no longer contains the item.
        Assert.Empty(purchaseOrder.Items);

        Assert.Equal(1, purchaseOrderRepository.GetByIdAsyncCallCount);
        Assert.Equal(purchaseOrderId, purchaseOrderRepository.LastGetByIdAsyncRequestId);
        Assert.Equal(1, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // Final-item removal (empty Draft allowed by the Domain)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_FinalItem_RemovalSucceedsAndSavesOnce()
    {
        // Arrange
        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrderWithItem(
            id: 7,
            supplierId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        var request = new RemovePurchaseOrderItemRequest(
            PurchaseOrderId: 7,
            ProductId: 10);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new RemovePurchaseOrderItemHandler(purchaseOrderRepository, unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value!.PurchaseOrderId);
        Assert.Equal(10, result.Value!.ProductId);

        // Empty item collection is allowed for a Draft aggregate.
        Assert.Empty(purchaseOrder.Items);

        Assert.Equal(1, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // Purchase order not found
    // =====================================================================

    [Fact]
    public async Task HandleAsync_PurchaseOrderNotFound_ReturnsNotFoundFailureAndDoesNotSave()
    {
        // Arrange
        var request = new RemovePurchaseOrderItemRequest(
            PurchaseOrderId: 42,
            ProductId: 10);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder: null);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new RemovePurchaseOrderItemHandler(purchaseOrderRepository, unitOfWork);

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

        var request = new RemovePurchaseOrderItemRequest(
            PurchaseOrderId: 7,
            ProductId: 99);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new RemovePurchaseOrderItemHandler(purchaseOrderRepository, unitOfWork);

        // Act
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(request));

        // Assert
        Assert.Equal("Purchase order item was not found.", exception.Message);

        Assert.Single(purchaseOrder.Items);
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

        var request = new RemovePurchaseOrderItemRequest(
            PurchaseOrderId: 7,
            ProductId: 10);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new RemovePurchaseOrderItemHandler(purchaseOrderRepository, unitOfWork);

        // Act
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(request));

        // Assert
        Assert.Equal("Only draft purchase orders can be modified.", exception.Message);
        Assert.Equal(PurchaseOrderStatus.Submitted, purchaseOrder.Status);

        Assert.Single(purchaseOrder.Items);
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

        var request = new RemovePurchaseOrderItemRequest(
            PurchaseOrderId: 7,
            ProductId: 10);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new RemovePurchaseOrderItemHandler(purchaseOrderRepository, unitOfWork);

        // Act
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(request));

        // Assert
        Assert.Equal("Only draft purchase orders can be modified.", exception.Message);
        Assert.Equal(PurchaseOrderStatus.Cancelled, purchaseOrder.Status);

        Assert.Single(purchaseOrder.Items);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }
}
