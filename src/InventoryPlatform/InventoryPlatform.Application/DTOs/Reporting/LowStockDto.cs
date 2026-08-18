namespace InventoryPlatform.Application.DTOs.Reporting;

public sealed record LowStockDto(
    int ProductId,
    string ProductName,
    string ProductSku,
    string? CategoryName,
    decimal QuantityOnHand);
