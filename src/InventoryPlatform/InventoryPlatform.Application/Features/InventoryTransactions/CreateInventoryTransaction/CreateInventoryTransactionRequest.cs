using System.ComponentModel.DataAnnotations;

namespace InventoryPlatform.Application.Features.InventoryTransactions.CreateInventoryTransaction;

public sealed class CreateInventoryTransactionRequest
{
    public int ProductId { get; set; }

    [Display(Name = "Transaction Type")]
    public TransactionType TransactionType { get; set; }

    public decimal Quantity { get; set; }

    [Display(Name = "Reference Number")]
    public string ReferenceNumber { get; set; } = string.Empty;

    public string? Remarks { get; set; }

    public DateTime TransactionDateUtc { get; set; } = DateTime.UtcNow;
}