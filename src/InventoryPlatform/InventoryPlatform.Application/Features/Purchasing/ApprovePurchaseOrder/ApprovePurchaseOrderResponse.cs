using InventoryPlatform.Domain.Enums;

namespace InventoryPlatform.Application.Features.Purchasing.ApprovePurchaseOrder;

public sealed record ApprovePurchaseOrderResponse(
    int Id,
    PurchaseOrderStatus Status);