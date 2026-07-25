namespace InventoryPlatform.Application.Features.InventoryTransactions.CreateInventoryTransaction;

public sealed record CreateInventoryTransactionResponse(
    int Id,
    decimal QuantityOnHand,
    string ReferenceNumber);