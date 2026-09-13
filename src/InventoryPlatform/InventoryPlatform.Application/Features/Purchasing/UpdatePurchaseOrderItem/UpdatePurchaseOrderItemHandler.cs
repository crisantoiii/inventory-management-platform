using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Domain.Entities;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Purchasing.UpdatePurchaseOrderItem;

public sealed class UpdatePurchaseOrderItemHandler
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePurchaseOrderItemHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdatePurchaseOrderItemResponse>> HandleAsync(
        UpdatePurchaseOrderItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(
            request.PurchaseOrderId,
            cancellationToken);

        if (purchaseOrder is null)
        {
            return Result<UpdatePurchaseOrderItemResponse>.Failure(
                PurchaseOrderErrors.NotFound);
        }

        purchaseOrder.UpdateItem(
            request.ProductId,
            request.Quantity,
            request.UnitCost);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var item = purchaseOrder.Items.Single(x => x.ProductId == request.ProductId);

        return Result<UpdatePurchaseOrderItemResponse>.Success(
            new UpdatePurchaseOrderItemResponse(
                purchaseOrder.Id,
                item.ProductId,
                item.Quantity,
                item.UnitCost));
    }
}
