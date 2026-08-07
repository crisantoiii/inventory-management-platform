using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Purchasing.ReceivePurchaseOrder;

public sealed class ReceivePurchaseOrderHandler
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReceivePurchaseOrderHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReceivePurchaseOrderResponse>> HandleAsync(
        ReceivePurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(
            request.PurchaseOrderId,
            cancellationToken);

        if (purchaseOrder is null)
        {
            return Result<ReceivePurchaseOrderResponse>.Failure(
                PurchaseOrderErrors.NotFound);
        }

        purchaseOrder.Receive(
            request.ProductId,
            request.Quantity);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ReceivePurchaseOrderResponse>.Success(
            new ReceivePurchaseOrderResponse(
                purchaseOrder.Id,
                purchaseOrder.Status));
    }
}