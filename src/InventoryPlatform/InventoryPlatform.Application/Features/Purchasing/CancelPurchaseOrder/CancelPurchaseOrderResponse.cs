using InventoryPlatform.Domain.Enums;

namespace InventoryPlatform.Application.Features.Purchasing.CancelPurchaseOrder;

public sealed record CancelPurchaseOrderResponse(
    int Id,
    PurchaseOrderStatus Status);
