namespace InventoryPlatform.Application.Features.Purchasing.ReceivePurchaseOrder;

public sealed record ReceivePurchaseOrderRequest(
    int PurchaseOrderId,
    int ProductId,
    decimal Quantity);