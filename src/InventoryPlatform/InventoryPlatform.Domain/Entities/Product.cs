using InventoryPlatform.Shared.Guards;
using InventoryPlatform.Domain.Common;

namespace InventoryPlatform.Domain.Entities;

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

    private Product()
    {
    }

    public Product(
        string sku,
        string name,
        string unit,
        decimal costPrice,
        decimal sellingPrice)
    {
        Sku = sku;

        Rename(name);
        ChangeCostPrice(costPrice);
        ChangeSellingPrice(sellingPrice);

        Unit = unit;
        IsActive = true;
    }

    public void Rename(string name)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));

        Name = name;
    }

    public void ChangeBarcode(string? barcode)
    {
        Barcode = barcode;
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    public void ChangeCostPrice(decimal costPrice)
    {
        Guard.AgainstNegative(costPrice, nameof(costPrice));

        CostPrice = costPrice;
    }

    public void ChangeSellingPrice(decimal sellingPrice)
    {
        Guard.AgainstNegative(sellingPrice, nameof(sellingPrice));

        SellingPrice = sellingPrice;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}

