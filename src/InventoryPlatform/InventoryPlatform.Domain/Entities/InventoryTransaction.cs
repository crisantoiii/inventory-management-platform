using InventoryPlatform.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace InventoryPlatform.Domain.Entities;

public sealed class InventoryTransaction : AuditableEntity
{

    public int ProductId { get; private set; }
    public Product Product { get; private set; } = default!;

    public TransactionType TransactionType { get; private set; }

    public decimal Quantity { get; private set; }

    public string ReferenceNumber { get; private set; } = string.Empty;

    public string? Remarks { get; private set; }

    public DateTime TransactionDateUtc { get; private set; }

    public InventoryTransaction(
        int productId,
        TransactionType transactionType,
        decimal quantity,
        string referenceNumber,
        string? remarks,
        DateTime transactionDateUtc)
    {
        ProductId = productId;
        TransactionType = transactionType;
        Quantity = quantity;
        ReferenceNumber = referenceNumber;
        Remarks = remarks;
        TransactionDateUtc = transactionDateUtc;
    }

    public bool IsStockIn => TransactionType == TransactionType.StockIn;

    public bool IsStockOut => TransactionType == TransactionType.StockOut;

    public bool IsAdjustment => TransactionType == TransactionType.Adjustment;

}
