namespace InventoryPlatform.Application.DTOs.Reporting;

public sealed record SupplierPurchaseAnalysisDto(
    int SupplierId,
    string SupplierName,
    DateOnly FirstOrderDate,
    DateOnly LastOrderDate,
    int PurchaseOrderCount,
    decimal TotalQuantity,
    decimal ReceivedQuantity,
    decimal RemainingQuantity,
    decimal TotalAmount);