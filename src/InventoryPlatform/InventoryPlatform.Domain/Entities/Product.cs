using InventoryPlatform.Shared.Guards;
using InventoryPlatform.Domain.Common;

namespace InventoryPlatform.Domain.Entities;

public sealed class Product : AuditableEntity
{
    public string Sku { get; private set; } = string.Empty;

    public string? Barcode { get; private set; } 

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int CategoryId { get; private set; }

    public Category Category { get; private set; } = default!;

    public int UnitId { get; private set; }

    public Unit Unit { get; private set; } = default!;

    public decimal QuantityOnHand { get; private set; }

    public decimal CostPrice { get; private set; }

    public decimal SellingPrice { get; private set; }

    public bool IsActive { get; private set; }

    private Product() { }

    public Product(
        string sku,
        string name,
        int categoryId,      
        int unitId,          
        decimal quantityOnHand,
        decimal costPrice,
        decimal sellingPrice)
    {
        SetSku(sku);
        Rename(name);
        ChangeCategory(categoryId); 
        ChangeUnit(unitId);
        ChangeQuantityOnHand(quantityOnHand);
        ChangeCostPrice(costPrice);
        ChangeSellingPrice(sellingPrice);

        IsActive = true;
    }

    public void Rename(string name)
    {
        Guard.AgainstNullOrWhiteSpace(name, nameof(name));
        Name = name;
    }

    public void ChangeCategory(int categoryId)
    {
        Guard.AgainstZeroOrNegative(categoryId, nameof(categoryId)); 
        CategoryId = categoryId;
    }

    public void ChangeUnit(int unitId)
    {
        Guard.AgainstZeroOrNegative(unitId, nameof(unitId));
        UnitId = unitId;
    }

    public void ChangeBarcode(string? barcode) => Barcode = barcode;
    public void UpdateDescription(string? description) => Description = description;

    public void ChangeQuantityOnHand(decimal quantityOnHand)
    {
        Guard.AgainstNegative(quantityOnHand, nameof(quantityOnHand));
        QuantityOnHand = quantityOnHand;
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

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    private void SetSku(string sku)
    {
        Guard.AgainstNullOrWhiteSpace(sku, nameof(sku));
        Sku = sku;
    }
}

