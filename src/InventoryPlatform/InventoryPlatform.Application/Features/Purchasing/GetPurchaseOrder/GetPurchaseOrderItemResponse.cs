namespace InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrder;

public sealed record GetPurchaseOrderItemResponse(
    int ProductId,
    string ProductSku,
    string ProductName,
    decimal Quantity,
    decimal UnitCost,
    decimal LineTotal,
    decimal ReceivedQuantity,
    decimal RemainingQuantity,
    bool IsFullyReceived);