using InventoryPlatform.Domain.Common;
using InventoryPlatform.Domain.Enums;

namespace InventoryPlatform.Domain.Entities;

public sealed class PurchaseOrder : BaseEntity
{
    private readonly List<PurchaseOrderItem> _items = new();

    private PurchaseOrder()
    {
    }

    public Guid SupplierId { get; private set; }

    public Supplier Supplier { get; private set; } = null!;

    public DateOnly OrderDate { get; private set; }

    public DateOnly? ExpectedDeliveryDate { get; private set; }

    public PurchaseOrderStatus Status { get; private set; }

    public string? Remarks { get; private set; }

    public IReadOnlyCollection<PurchaseOrderItem> Items => _items.AsReadOnly();
}