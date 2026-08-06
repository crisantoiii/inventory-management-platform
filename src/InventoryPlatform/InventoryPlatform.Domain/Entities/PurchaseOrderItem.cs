using InventoryPlatform.Domain.Common;

namespace InventoryPlatform.Domain.Entities;

public sealed class PurchaseOrderItem : BaseEntity
{
    private PurchaseOrderItem()
    {
    }

    public int PurchaseOrderId { get; private set; }

    public PurchaseOrder PurchaseOrder { get; private set; } = null!;

    public int ProductId { get; private set; }

    public Product Product { get; private set; } = null!;

    public decimal Quantity { get; private set; }

    public decimal UnitCost { get; private set; }

    public decimal ReceivedQuantity { get; private set; }

    /// <summary>
    /// Gets the total amount for this purchase order line.
    /// </summary>
    public decimal LineTotal => Quantity * UnitCost;

    /// <summary>
    /// Gets the remaining quantity to be received.
    /// </summary>
    public decimal RemainingQuantity => Quantity - ReceivedQuantity;

    /// <summary>
    /// Gets a value indicating whether this line has been fully received.
    /// </summary>
    public bool IsFullyReceived => RemainingQuantity == 0;

    public static PurchaseOrderItem Create(
        int purchaseOrderId,
        int productId,
        decimal quantity,
        decimal unitCost)
    {
        return new PurchaseOrderItem
        {
            PurchaseOrderId = purchaseOrderId,
            ProductId = productId,
            Quantity = quantity,
            UnitCost = unitCost,
            ReceivedQuantity = 0
        };
    }

    /// <summary>
    /// Receives inventory for this purchase order line.
    /// </summary>
    /// <param name="quantity">
    /// Quantity received from the supplier.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the received quantity is invalid.
    /// </exception>
    public void Receive(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException(
                "Received quantity must be greater than zero.");
        }

        if (ReceivedQuantity + quantity > Quantity)
        {
            throw new InvalidOperationException(
                "Received quantity cannot exceed ordered quantity.");
        }

        ReceivedQuantity += quantity;
    }

    /// <summary>
    /// Updates the purchase order line.
    /// </summary>
    public void Update(
        decimal quantity,
        decimal unitCost)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException(
                "Quantity must be greater than zero.");
        }

        if (unitCost < 0)
        {
            throw new InvalidOperationException(
                "Unit cost cannot be negative.");
        }

        Quantity = quantity;
        UnitCost = unitCost;
    }
}