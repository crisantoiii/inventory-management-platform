namespace InventoryPlatform.Application.Features.Purchasing.CreatePurchaseOrder;

public sealed record CreatePurchaseOrderItemRequest(
    int ProductId,
    decimal Quantity,
    decimal UnitCost);