using InventoryPlatform.Shared.Paging;

namespace InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrders;

public sealed record GetPurchaseOrdersResponse(
    PagedResult<GetPurchaseOrderSummaryResponse> PurchaseOrders);
