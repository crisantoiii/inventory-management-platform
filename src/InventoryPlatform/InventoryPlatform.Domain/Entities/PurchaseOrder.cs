using InventoryPlatform.Domain.Common;
using InventoryPlatform.Domain.Enums;

namespace InventoryPlatform.Domain.Entities;

public sealed class PurchaseOrder : BaseEntity
{
    private readonly List<PurchaseOrderItem> _items = new();

    private PurchaseOrder()
    {
    }

    public int SupplierId { get; private set; }

    public Supplier Supplier { get; private set; } = null!;

    public DateOnly OrderDate { get; private set; }

    public DateOnly? ExpectedDeliveryDate { get; private set; }

    public PurchaseOrderStatus Status { get; private set; }

    public string? Remarks { get; private set; }

    public IReadOnlyCollection<PurchaseOrderItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(x => x.LineTotal);

    public static PurchaseOrder Create(
        int supplierId,
        DateOnly orderDate,
        DateOnly? expectedDeliveryDate,
        string? remarks)
    {
        return new PurchaseOrder
        {
            SupplierId = supplierId,
            OrderDate = orderDate,
            ExpectedDeliveryDate = expectedDeliveryDate,
            Remarks = remarks,
            Status = PurchaseOrderStatus.Draft
        };
    }

    public void AddItem(
        int productId,
        decimal quantity,
        decimal unitCost)
    {
        EnsureDraft();

        if (_items.Any(x => x.ProductId == productId))
        {
            throw new InvalidOperationException(
                "The product already exists in this purchase order.");
        }

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

        _items.Add(
            PurchaseOrderItem.Create(
                Id,
                productId,
                quantity,
                unitCost));
    }

    public void UpdateItem(
    int productId,
    decimal quantity,
    decimal unitCost)
    {
        EnsureDraft();

        var item = _items.SingleOrDefault(x => x.ProductId == productId);

        if (item is null)
        {
            throw new InvalidOperationException(
                "Purchase order item was not found.");
        }

        item.Update(
            quantity,
            unitCost);
    }

    public void RemoveItem(int productId)
    {
        EnsureDraft();

        var item = _items.SingleOrDefault(x => x.ProductId == productId);

        if (item is null)
        {
            throw new InvalidOperationException(
                "Purchase order item was not found.");
        }

        _items.Remove(item);
    }

    private void EnsureDraft()
    {
        if (Status != PurchaseOrderStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only draft purchase orders can be modified.");
        }
    }
}