using InventoryPlatform.Application.Features.Purchasing;
using InventoryPlatform.Application.Features.Purchasing.SubmitPurchaseOrder;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Domain.Exceptions;
using InventoryPlatform.Shared.Results;
using InventoryPlatform.UnitTests.TestSupport.Purchasing;
using Xunit;

namespace InventoryPlatform.UnitTests.Application.Purchasing;

public sealed class SubmitPurchaseOrderHandlerTests
{
    // =====================================================================
    // Purchase order not found
    // =====================================================================

    [Fact]
    public async Task HandleAsync_PurchaseOrderNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        var request = new SubmitPurchaseOrderRequest(Id: 42);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder: null);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new SubmitPurchaseOrderHandler(purchaseOrderRepository, unitOfWork);

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
    // Valid Draft with at least one item
    // =====================================================================

    [Fact]
    public async Task HandleAsync_ValidDraftWithItems_SubmitsAndReturnsSubmittedResponse()
    {
        // Arrange
        const int purchaseOrderId = 7;
        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrderWithItem(
            id: purchaseOrderId,
            supplierId: 1,
            productId: 10,
            quantity: 5m,
            unitCost: 25.00m);

        var request = new SubmitPurchaseOrderRequest(Id: purchaseOrderId);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new SubmitPurchaseOrderHandler(purchaseOrderRepository, unitOfWork);

        // Act
        var result = await handler.HandleAsync(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);

        var response = result.Value!;
        Assert.Equal(purchaseOrderId, response.Id);
        Assert.Equal(PurchaseOrderStatus.Submitted, response.Status);

        // The loaded (tracked) aggregate itself transitioned.
        Assert.Equal(PurchaseOrderStatus.Submitted, purchaseOrder.Status);

        Assert.Equal(1, purchaseOrderRepository.GetByIdAsyncCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_ValidDraftWithItems_SaveOccursAfterSuccessfulTransition()
    {
        // Arrange
        var callOrder = new CallOrder();
        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrderWithItem(
            id: 7,
            supplierId: 1);

        var request = new SubmitPurchaseOrderRequest(Id: 7);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(
            purchaseOrder: purchaseOrder,
            callOrder: callOrder);
        var unitOfWork = new FakeUnitOfWork(callOrder: callOrder);

        var handler = new SubmitPurchaseOrderHandler(purchaseOrderRepository, unitOfWork);

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
    // Empty Draft (Domain precondition: at least one item)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_EmptyDraft_DomainExceptionPropagatesAndDoesNotSave()
    {
        // Arrange
        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrder(
            id: 7,
            supplierId: 1);
        // No items added — Submit's Domain precondition is violated.

        var request = new SubmitPurchaseOrderRequest(Id: 7);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new SubmitPurchaseOrderHandler(purchaseOrderRepository, unitOfWork);

        // Act / Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => handler.HandleAsync(request));

        Assert.Contains("at least one item", exception.Message);

        // The aggregate remains in Draft and nothing was persisted.
        Assert.Equal(PurchaseOrderStatus.Draft, purchaseOrder.Status);
        Assert.Equal(1, purchaseOrderRepository.GetByIdAsyncCallCount);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    // =====================================================================
    // Invalid non-Draft state (Domain precondition: only Draft can submit)
    // =====================================================================

    [Fact]
    public async Task HandleAsync_AlreadySubmittedPurchaseOrder_DomainExceptionPropagatesAndDoesNotSave()
    {
        // Arrange — real Domain transition: Draft + item -> Submit().
        var purchaseOrder = PurchasingTestData.CreateSubmittedPurchaseOrder(
            id: 7,
            supplierId: 1);

        var request = new SubmitPurchaseOrderRequest(Id: 7);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new SubmitPurchaseOrderHandler(purchaseOrderRepository, unitOfWork);

        // Act / Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => handler.HandleAsync(request));

        Assert.Contains("Only draft", exception.Message);

        // The aggregate remains in its prior (Submitted) state; nothing was persisted.
        Assert.Equal(PurchaseOrderStatus.Submitted, purchaseOrder.Status);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_AlreadyApprovedPurchaseOrder_DomainExceptionPropagatesAndDoesNotSave()
    {
        // Arrange — real Domain transitions: Draft + item -> Submit() -> Approve().
        var purchaseOrder = PurchasingTestData.CreateApprovedPurchaseOrder(
            id: 7,
            supplierId: 1);

        var request = new SubmitPurchaseOrderRequest(Id: 7);

        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new SubmitPurchaseOrderHandler(purchaseOrderRepository, unitOfWork);

        // Act / Assert
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => handler.HandleAsync(request));

        Assert.Contains("Only draft", exception.Message);

        Assert.Equal(PurchaseOrderStatus.Approved, purchaseOrder.Status);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }
}
