using InventoryPlatform.Domain.Common;

namespace InventoryPlatform.Domain.Entities
{
    public sealed class Product : AuditableEntity
    {
        public string Sku { get; private set; } = string.Empty;

        public string? Barcode { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string? Description { get; private set; }

        public string Unit { get; private set; } = string.Empty;

        public decimal CostPrice { get; private set; }

        public decimal SellingPrice { get; private set; }

        public bool IsActive { get; private set; }
    }
}
