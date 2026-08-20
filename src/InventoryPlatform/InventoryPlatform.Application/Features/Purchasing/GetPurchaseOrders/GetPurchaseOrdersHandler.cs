using InventoryPlatform.Application.Interfaces.Persistence;
using InventoryPlatform.Shared.Paging;
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
        var query = new PagedQuery
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Search = request.Search,
            SortBy = request.SortBy,
            Descending = request.Descending
        };

        var purchaseOrders = await _purchaseOrderRepository
            .GetPurchaseOrdersAsync(
                query,
                request.FromDate,
                request.ToDate,
                request.PurchaseOrderStatus,
                cancellationToken);

        var response = new GetPurchaseOrdersResponse(
            new PagedResult<GetPurchaseOrderSummaryResponse>
            {
                Items = purchaseOrders.Items
                    .Select(po => new GetPurchaseOrderSummaryResponse(
                        po.Id,
                        po.Supplier.Name,
                        po.OrderDate,
                        po.Status,
                        po.TotalAmount))
                    .ToList(),
                Page = purchaseOrders.Page,
                PageSize = purchaseOrders.PageSize,
                TotalCount = purchaseOrders.TotalCount
            });

        return Result<GetPurchaseOrdersResponse>.Success(response);
    }
}
