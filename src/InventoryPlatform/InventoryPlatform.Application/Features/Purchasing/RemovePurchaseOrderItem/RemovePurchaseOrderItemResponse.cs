namespace InventoryPlatform.Application.Features.Purchasing.RemovePurchaseOrderItem;

public sealed record RemovePurchaseOrderItemResponse(
    int PurchaseOrderId,
    int ProductId);
