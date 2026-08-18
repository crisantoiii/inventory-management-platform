namespace InventoryPlatform.Application.DTOs.Reporting;

public sealed record ProductReportDto(
    int ProductId,
    string ProductName,
    string ProductSku,
    string? CategoryName,
    string? UnitName,
    decimal QuantityOnHand,
    decimal CostPrice,
    decimal SellingPrice,
    bool IsActive);
