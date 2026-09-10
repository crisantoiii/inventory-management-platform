namespace InventoryPlatform.Application.Features.Purchasing.UpdatePurchaseOrderItem;

public sealed record UpdatePurchaseOrderItemRequest(
    int PurchaseOrderId,
    int ProductId,
    decimal Quantity,
    decimal UnitCost);
