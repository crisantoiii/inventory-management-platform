using InventoryPlatform.Domain.Enums;


namespace InventoryPlatform.Application.Features.Purchasing.CreatePurchaseOrder;

public sealed record CreatePurchaseOrderResponse(
    int Id,
    PurchaseOrderStatus Status);
