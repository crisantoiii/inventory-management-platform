using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Purchasing.CancelPurchaseOrder;

public sealed class CancelPurchaseOrderHandler
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelPurchaseOrderHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CancelPurchaseOrderResponse>> HandleAsync(
        CancelPurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (purchaseOrder is null)
        {
            return Result<CancelPurchaseOrderResponse>.Failure(
                PurchaseOrderErrors.NotFound);
        }

        purchaseOrder.Cancel();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CancelPurchaseOrderResponse>.Success(
            new CancelPurchaseOrderResponse(
                purchaseOrder.Id,
                purchaseOrder.Status));
    }
}
