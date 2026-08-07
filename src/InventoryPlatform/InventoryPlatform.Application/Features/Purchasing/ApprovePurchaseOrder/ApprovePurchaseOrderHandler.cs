using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Purchasing.ApprovePurchaseOrder;

public sealed class ApprovePurchaseOrderHandler
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApprovePurchaseOrderHandler(
        IPurchaseOrderRepository purchaseOrderRepository,
        IUnitOfWork unitOfWork)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ApprovePurchaseOrderResponse>> HandleAsync(
        ApprovePurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (purchaseOrder is null)
        {
            return Result<ApprovePurchaseOrderResponse>.Failure(
                PurchaseOrderErrors.NotFound);
        }

        purchaseOrder.Approve();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ApprovePurchaseOrderResponse>.Success(
            new ApprovePurchaseOrderResponse(
                purchaseOrder.Id,
                purchaseOrder.Status));
    }
}