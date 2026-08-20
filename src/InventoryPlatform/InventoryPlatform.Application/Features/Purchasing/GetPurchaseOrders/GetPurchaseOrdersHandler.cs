using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrders;

public sealed class GetPurchaseOrdersHandler
{
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;

    public GetPurchaseOrdersHandler(
        IPurchaseOrderRepository purchaseOrderRepository)
    {
        _purchaseOrderRepository = purchaseOrderRepository;
    }

    public async Task<Result<GetPurchaseOrdersResponse>> HandleAsync(
        GetPurchaseOrdersRequest request,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrders = await _purchaseOrderRepository
            .GetPurchaseOrdersAsync(
                request.Search,
                request.FromDate,
                request.ToDate,
                request.Status,
                cancellationToken);

        var response = new GetPurchaseOrdersResponse(
            purchaseOrders
                .Select(po => new GetPurchaseOrderSummaryResponse(
                    po.Id,
                    po.Supplier.Name,
                    po.OrderDate,
                    po.Status,
                    po.TotalAmount))
                .ToList());

        return Result<GetPurchaseOrdersResponse>.Success(response);
    }
}
