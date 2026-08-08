using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrder;

public sealed class GetPurchaseOrderHandler
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;

    public GetPurchaseOrderHandler(
        IPurchaseOrderRepository purchaseOrderRepository)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
    }

    public async Task<Result<GetPurchaseOrderResponse>> HandleAsync(
        GetPurchaseOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (purchaseOrder is null)
        {
            return Result<GetPurchaseOrderResponse>.Failure(
                PurchaseOrderErrors.NotFound);
        }

        var response = new GetPurchaseOrderResponse(
            purchaseOrder.Id,
            purchaseOrder.SupplierId,
            purchaseOrder.Supplier.Name,
            purchaseOrder.OrderDate,
            purchaseOrder.ExpectedDeliveryDate,
            purchaseOrder.Status,
            purchaseOrder.Remarks,
            purchaseOrder.TotalAmount,
            purchaseOrder.Items
                .Select(item => new GetPurchaseOrderItemResponse(
                    item.ProductId,
                    item.Product.Sku,
                    item.Product.Name,
                    item.Quantity,
                    item.UnitCost,
                    item.LineTotal,
                    item.ReceivedQuantity,
                    item.RemainingQuantity,
                    item.IsFullyReceived))
                .ToList());

        return Result<GetPurchaseOrderResponse>.Success(response);
    }
}