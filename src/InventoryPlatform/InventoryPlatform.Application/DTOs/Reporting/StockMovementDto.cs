using InventoryPlatform.Domain.Enums;

namespace InventoryPlatform.Application.DTOs.Reporting;

public sealed record StockMovementDto(
    int Id,
    int ProductId,
    string ProductName,
    string ProductSku,
    TransactionType TransactionType,
    decimal Quantity,
    string ReferenceNumber,
    string? Remarks,
    DateTime TransactionDateUtc);