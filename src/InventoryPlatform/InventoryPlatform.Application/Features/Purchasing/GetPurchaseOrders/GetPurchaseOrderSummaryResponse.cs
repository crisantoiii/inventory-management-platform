using InventoryPlatform.Domain.Enums;

namespace InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrders;

public sealed record GetPurchaseOrderSummaryResponse(
    int Id,
    string SupplierName,
    DateOnly OrderDate,
    PurchaseOrderStatus Status,
    decimal TotalAmount);