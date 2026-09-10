namespace InventoryPlatform.Application.Features.Purchasing.RemovePurchaseOrderItem;

public sealed record RemovePurchaseOrderItemRequest(
    int PurchaseOrderId,
    int ProductId);
