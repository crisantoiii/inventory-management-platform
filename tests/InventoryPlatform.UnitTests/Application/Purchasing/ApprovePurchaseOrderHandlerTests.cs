using InventoryPlatform.Application.Features.Purchasing;
using InventoryPlatform.Application.Features.Purchasing.ApprovePurchaseOrder;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Domain.Exceptions;
using InventoryPlatform.Shared.Results;
using InventoryPlatform.UnitTests.TestSupport.Purchasing;
using Xunit;

namespace InventoryPlatform.UnitTests.Application.Purchasing;

public sealed class ApprovePurchaseOrderHandlerTests
{
    // =====================================================================
    // Purchase order not found
    // =====================================================================

    [Fact]
    public async Task HandleAsync_PurchaseOrderNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var request = new ApprovePurchaseOrderRequest(Id: 42);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder: null);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ApprovePurchaseOrderHandler(purchaseOrderRepository, unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(PurchaseOrderErrors.NotFound, result.Error);
        Assert.Equal("PurchaseOrder.NotFound", result.Error.Code);
        Assert.Equal("Purchase order not found.", result.Error.Message);

        Assert.Equal(1, purchaseOrderRepository.GetByIdAsyncCallCount);
        Assert.Equal(42, purchaseOrderRepository.LastGetByIdAsyncRequestId);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // Valid Submitted purchase order
    // =====================================================================

    [Fact]
    public async Task HandleAsync_ValidSubmittedPurchaseOrder_ApprovesAndReturnsApprovedResponse()
    {
        // Arrange — real Domain transition: Draft + item -> Submit().
        const int purchaseOrderId = 7;
        var purchaseOrder = PurchasingTestData.CreateSubmittedPurchaseOrder(
            id: purchaseOrderId,
            supplierId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        var request = new ApprovePurchaseOrderRequest(Id: purchaseOrderId);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ApprovePurchaseOrderHandler(purchaseOrderRepository, unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);

        var response = result.Value!;
        Assert.Equal(purchaseOrderId, response.Id);
        Assert.Equal(PurchaseOrderStatus.Approved, response.Status);

        // The loaded (tracked) aggregate itself transitioned.
        Assert.Equal(PurchaseOrderStatus.Approved, purchaseOrder.Status);

        Assert.Equal(1, purchaseOrderRepository.GetByIdAsyncCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_ValidSubmittedPurchaseOrder_SaveOccursAfterSuccessfulTransition()
    {
        // Arrange
        var callOrder = new CallOrder();
        var purchaseOrder = PurchasingTestData.CreateSubmittedPurchaseOrder(
            id: 7,
            supplierId: 1);

        var request = new ApprovePurchaseOrderRequest(Id: 7);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(
            purchaseOrder: purchaseOrder,
            callOrder: callOrder);
        var unitOfWork = new FakeUnitOfWork(callOrder: callOrder);

        var handler = new ApprovePurchaseOrderHandler(purchaseOrderRepository, unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);

        var events = callOrder.Events.ToList();
        Assert.Equal(
            new[] { "PurchaseOrderRepository.GetByIdAsync", "UnitOfWork.SaveChangesAsync" },
            events);
    }

    // =====================================================================
    // Invalid Draft state (Domain precondition: only Submitted can approve)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_DraftPurchaseOrder_DomainExceptionPropagatesAndDoesNotSave()
    {
        // Arrange — Draft WITH items: the violated precondition is the state
        // requirement (only Submitted can approve), not the item requirement.
        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrderWithItem(
            id: 7,
            supplierId: 1);

        var request = new ApprovePurchaseOrderRequest(Id: 7);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ApprovePurchaseOrderHandler(purchaseOrderRepository, unitOfWork);

        // Act / Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => handler.HandleAsync(request));

        Assert.Contains("Only submitted", exception.Message);

        // The aggregate remains in Draft and nothing was persisted.
        Assert.Equal(PurchaseOrderStatus.Draft, purchaseOrder.Status);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // Invalid later states, constructible through real Domain transitions
    // =====================================================================

    [Fact]
    public async Task HandleAsync_AlreadyApprovedPurchaseOrder_DomainExceptionPropagatesAndDoesNotSave()
    {
        // Arrange — real Domain transitions: Draft + item -> Submit() -> Approve().
        var purchaseOrder = PurchasingTestData.CreateApprovedPurchaseOrder(
            id: 7,
            supplierId: 1);

        var request = new ApprovePurchaseOrderRequest(Id: 7);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ApprovePurchaseOrderHandler(purchaseOrderRepository, unitOfWork);

        // Act / Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => handler.HandleAsync(request));

        Assert.Contains("Only submitted", exception.Message);

        // The aggregate remains Approved; nothing was persisted.
        Assert.Equal(PurchaseOrderStatus.Approved, purchaseOrder.Status);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_ReceivingPurchaseOrder_DomainExceptionPropagatesAndDoesNotSave()
    {
        // Arrange — real Domain transitions: Draft + item -> Submit() -> Approve()
        // -> partial Receive() moves the aggregate into Receiving.
        var purchaseOrder = PurchasingTestData.CreateApprovedPurchaseOrder(
            id: 7,
            supplierId: 1,
            productId: 10,
            quantity: 10m);
        purchaseOrder.Receive(productId: 10, quantity: 3m);
        Assert.Equal(PurchaseOrderStatus.Receiving, purchaseOrder.Status);

        var request = new ApprovePurchaseOrderRequest(Id: 7);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new ApprovePurchaseOrderHandler(purchaseOrderRepository, unitOfWork);

        // Act / Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => handler.HandleAsync(request));

        Assert.Contains("Only submitted", exception.Message);

        Assert.Equal(PurchaseOrderStatus.Receiving, purchaseOrder.Status);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }
}
