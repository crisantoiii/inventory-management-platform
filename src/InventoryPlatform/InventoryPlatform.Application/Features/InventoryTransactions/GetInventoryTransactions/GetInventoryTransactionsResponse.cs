namespace InventoryPlatform.Application.Features.InventoryTransactions.GetInventoryTransactions;

public sealed record GetInventoryTransactionsResponse
{
    public int Id { get; init; }

    public int ProductId { get; init; }

    public string ProductName { get; init; }

    public string Sku { get; init; }

    public TransactionType TransactionType { get; init; }

    public decimal Quantity { get; init; }

    public string ReferenceNumber { get; init; } = string.Empty;

    public string? Remarks { get; init; }

    public DateTime TransactionDateUtc { get; init; }
}