using InventoryPlatform.Domain.Common;

namespace InventoryPlatform.Domain.Entities;

public sealed class PurchaseOrderItem : BaseEntity
{
    private PurchaseOrderItem()
    {
    }

    public Guid PurchaseOrderId { get; private set; }

    public PurchaseOrder PurchaseOrder { get; private set; } = null!;

    public Guid ProductId { get; private set; }

    public Product Product { get; private set; } = null!;

    public decimal Quantity { get; private set; }

    public decimal UnitCost { get; private set; }

    public decimal ReceivedQuantity { get; private set; }
}