using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Purchasing.SubmitPurchaseOrder;

public sealed class SubmitPurchaseOrderHandler
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitPurchaseOrderHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SubmitPurchaseOrderResponse>> HandleAsync(
        SubmitPurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (purchaseOrder is null)
        {
            return Result<SubmitPurchaseOrderResponse>.Failure(
                PurchaseOrderErrors.NotFound);
        }

        purchaseOrder.Submit();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SubmitPurchaseOrderResponse>.Success(
            new SubmitPurchaseOrderResponse(
                purchaseOrder.Id,
                purchaseOrder.Status));
    }
}