using InventoryPlatform.Domain.Common;
using InventoryPlatform.Domain.Exceptions;

namespace InventoryPlatform.Domain.Entities;

public sealed class StockCountLine : AuditableEntity
{
    public int StockCountSessionId { get; private set; }
    public StockCountSession StockCountSession { get; private set; } = default!;

    public int ProductId { get; private set; }
    public Product Product { get; private set; } = default!;

    public decimal ExpectedQuantity { get; private set; }

    public decimal? CountedQuantity { get; private set; }

    public int? InventoryTransactionId { get; private set; }
    public InventoryTransaction? InventoryTransaction { get; private set; }

    private StockCountLine()
    {
    }

    public static StockCountLine Create(int sessionId, int productId, decimal expectedQuantity)
    {
        if (sessionId <= 0) throw new DomainException("SessionId must be positive.");
        if (productId <= 0) throw new DomainException("ProductId must be positive.");
        if (expectedQuantity < 0) throw new DomainException("ExpectedQuantity must be non-negative.");

        return new StockCountLine
        {
            StockCountSessionId = sessionId,
            ProductId = productId,
            ExpectedQuantity = expectedQuantity,
            CountedQuantity = null,
            InventoryTransactionId = null
        };
    }

    internal void SetCountedQuantity(decimal quantity, bool allowBaselineMutationContext = false)
    {
        if (quantity < 0) throw new DomainException("CountedQuantity must be non-negative.");
        CountedQuantity = quantity;
    }

    internal void MarkPosted(int inventoryTransactionId)
    {
        if (inventoryTransactionId <= 0) throw new DomainException("InventoryTransactionId must be positive.");
        InventoryTransactionId = inventoryTransactionId;
    }
}
