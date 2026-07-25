namespace InventoryPlatform.Application.Features.InventoryTransactions.GetInventoryTransaction;

public sealed record GetInventoryTransactionResponse(
    int Id,
    int ProductId,
    string ProductName,
    string ProductSku,
    TransactionType TransactionType,
    decimal Quantity,
    string ReferenceNumber,
    string? Remarks,
    DateTime TransactionDateUtc);