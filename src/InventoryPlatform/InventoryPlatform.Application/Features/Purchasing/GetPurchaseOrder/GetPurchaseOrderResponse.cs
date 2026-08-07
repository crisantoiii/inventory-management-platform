using InventoryPlatform.Domain.Enums;

namespace InventoryPlatform.Application.Features.Purchasing.GetPurchaseOrder;

public sealed record GetPurchaseOrderResponse(
    int Id,
    int SupplierId,
    string SupplierName,
    DateOnly OrderDate,
    DateOnly? ExpectedDeliveryDate,
    PurchaseOrderStatus Status,
    string? Remarks,
    decimal TotalAmount,
    IReadOnlyCollection<GetPurchaseOrderItemResponse> Items);