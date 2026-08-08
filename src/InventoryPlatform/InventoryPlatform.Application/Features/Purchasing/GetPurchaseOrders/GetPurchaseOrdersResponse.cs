namespace InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrders;

public sealed record GetPurchaseOrdersResponse(
    IReadOnlyCollection<GetPurchaseOrderSummaryResponse> PurchaseOrders);