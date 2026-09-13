using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Purchasing.RemovePurchaseOrderItem;

public sealed class RemovePurchaseOrderItemHandler
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemovePurchaseOrderItemHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RemovePurchaseOrderItemResponse>> HandleAsync(
        RemovePurchaseOrderItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(
            request.PurchaseOrderId,
            cancellationToken);

        if (purchaseOrder is null)
        {
            return Result<RemovePurchaseOrderItemResponse>.Failure(
                PurchaseOrderErrors.NotFound);
        }

        purchaseOrder.RemoveItem(request.ProductId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RemovePurchaseOrderItemResponse>.Success(
            new RemovePurchaseOrderItemResponse(
                purchaseOrder.Id,
                request.ProductId));
    }
}
