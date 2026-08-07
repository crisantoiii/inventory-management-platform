using InventoryPlatform.Domain.Enums;

namespace InventoryPlatform.Application.Features.Purchasing.SubmitPurchaseOrder;

public sealed record SubmitPurchaseOrderResponse(
    int Id,
    PurchaseOrderStatus Status);