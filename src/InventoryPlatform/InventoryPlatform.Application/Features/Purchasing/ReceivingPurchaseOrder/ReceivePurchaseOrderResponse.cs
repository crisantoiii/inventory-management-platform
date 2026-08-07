using InventoryPlatform.Domain.Enums;

namespace InventoryPlatform.Application.Features.Purchasing.ReceivePurchaseOrder;

public sealed record ReceivePurchaseOrderResponse(
    int PurchaseOrderId,
    PurchaseOrderStatus Status);