using InventoryPlatform.Application.Features.Purchasing;
using InventoryPlatform.Application.Features.Purchasing.CancelPurchaseOrder;
using InventoryPlatform.Domain.Enums;
using InventoryPlatform.Domain.Exceptions;
using InventoryPlatform.UnitTests.TestSupport.Purchasing;
using Xunit;

namespace InventoryPlatform.UnitTests.Application.Purchasing;

public sealed class CancelPurchaseOrderHandlerTests
{
    [Fact]
    public async Task HandleAsync_DraftPurchaseOrder_CancelsAndReturnsCancelledResponse()
    {
        const int purchaseOrderId = 7;
        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrderWithItem(
            id: purchaseOrderId,
            supplierId: 1);
        var purchaseOrderRepository = new FakePurchaseOrderRepository(purchaseOrder);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CancelPurchaseOrderHandler(purchaseOrderRepository, unitOfWork);

        var result = await handler.HandleAsync(
            new CancelPurchaseOrderRequest(purchaseOrderId));

        Assert.True(result.IsSuccess);
        Assert.Equal(purchaseOrderId, result.Value!.Id);
        Assert.Equal(PurchaseOrderStatus.Cancelled, result.Value.Status);
        Assert.Equal(PurchaseOrderStatus.Cancelled, purchaseOrder.Status);
        Assert.Equal(1, purchaseOrderRepository.GetByIdAsyncCallCount);
        Assert.Equal(1, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Fact]
    public async Task HandleAsync_SubmittedPurchaseOrder_CancelsAndSavesOnce()
    {
        var callOrder = new CallOrder();
        var purchaseOrder = PurchasingTestData.CreateSubmittedPurchaseOrder(id: 7);
        var purchaseOrderRepository = new FakePurchaseOrderRepository(
            purchaseOrder,
            callOrder: callOrder);
        var unitOfWork = new FakeUnitOfWork(callOrder: callOrder);
        var handler = new CancelPurchaseOrderHandler(purchaseOrderRepository, unitOfWork);

        var result = await handler.HandleAsync(
            new CancelPurchaseOrderRequest(7));

        Assert.True(result.IsSuccess);
        Assert.Equal(PurchaseOrderStatus.Cancelled, result.Value!.Status);
        Assert.Equal(1, unitOfWork.SaveChangesAsyncCallCount);
        Assert.Equal(
            new[]
            {
                "PurchaseOrderRepository.GetByIdAsync",
                "UnitOfWork.SaveChangesAsync"
            },
            callOrder.Events);
    }

    [Fact]
    public async Task HandleAsync_PurchaseOrderNotFound_ReturnsNotFoundFailureAndDoesNotSave()
    {
        var purchaseOrderRepository = new FakePurchaseOrderRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CancelPurchaseOrderHandler(purchaseOrderRepository, unitOfWork);

        var result = await handler.HandleAsync(
            new CancelPurchaseOrderRequest(42));

        Assert.True(result.IsFailure);
        Assert.Equal(PurchaseOrderErrors.NotFound, result.Error);
        Assert.Equal(1, purchaseOrderRepository.GetByIdAsyncCallCount);
        Assert.Equal(42, purchaseOrderRepository.LastGetByIdAsyncRequestId);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    [Theory]
    [InlineData("Approved")]
    [InlineData("Receiving")]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    public async Task HandleAsync_ForbiddenState_PropagatesDomainExceptionAndDoesNotSave(
        string state)
    {
        var purchaseOrder = CreatePurchaseOrderInState(state);
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CancelPurchaseOrderHandler(
            new FakePurchaseOrderRepository(purchaseOrder),
            unitOfWork);

        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new CancelPurchaseOrderRequest(7)));

        Assert.Equal(
            "Only draft or submitted purchase orders can be cancelled.",
            exception.Message);
        Assert.Equal(ParseStatus(state), purchaseOrder.Status);
        Assert.Equal(0, unitOfWork.SaveChangesAsyncCallCount);
    }

    private static InventoryPlatform.Domain.Entities.PurchaseOrder
        CreatePurchaseOrderInState(string state)
    {
        var purchaseOrder = PurchasingTestData.CreateDraftPurchaseOrderWithItem(
            id: 7,
            supplierId: 1);

        switch (state)
        {
            case "Approved":
                purchaseOrder.Submit();
                purchaseOrder.Approve();
                break;
            case "Receiving":
                purchaseOrder.Submit();
                purchaseOrder.Approve();
                purchaseOrder.Receive(productId: 10, quantity: 3m);
                break;
            case "Completed":
                purchaseOrder.Submit();
                purchaseOrder.Approve();
                purchaseOrder.Receive(productId: 10, quantity: 5m);
                break;
            case "Cancelled":
                purchaseOrder.Cancel();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }

        return purchaseOrder;
    }

    private static PurchaseOrderStatus ParseStatus(string state) =>
        Enum.Parse<PurchaseOrderStatus>(state);
}
