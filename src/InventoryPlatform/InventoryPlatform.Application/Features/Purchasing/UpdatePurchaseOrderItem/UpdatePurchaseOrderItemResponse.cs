namespace InventoryPlatform.Application.Features.Purchasing.UpdatePurchaseOrderItem;

public sealed record UpdatePurchaseOrderItemResponse(
    int PurchaseOrderId,
    int ProductId,
    decimal Quantity,
    decimal UnitCost);
