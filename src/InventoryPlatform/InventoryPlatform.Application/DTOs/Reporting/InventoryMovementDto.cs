namespace InventoryPlatform.Application.DTOs.Reporting;

public sealed record InventoryMovementDto(
    int ProductId,
    string ProductName,
    string ProductSku,
    decimal OpeningQuantity,
    decimal StockInQuantity,
    decimal StockOutQuantity,
    decimal AdjustmentQuantity,
    decimal ClosingQuantity);
