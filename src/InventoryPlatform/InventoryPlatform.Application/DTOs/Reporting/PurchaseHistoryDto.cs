using InventoryPlatform.Domain.Enums;

namespace InventoryPlatform.Application.DTOs.Reporting;

public sealed record PurchaseHistoryDto(
    int PurchaseOrderId,
    string SupplierName,
    DateOnly OrderDate,
    PurchaseOrderStatus Status,
    decimal TotalAmount,
    decimal TotalQuantity,
    decimal ReceivedQuantity,
    decimal RemainingQuantity);