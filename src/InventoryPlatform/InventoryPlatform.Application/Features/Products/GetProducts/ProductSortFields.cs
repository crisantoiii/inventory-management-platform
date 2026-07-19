using InventoryPlatform.Domain.Entities;

namespace InventoryPlatform.Application.Features.Products.GetProducts;

public static class ProductSortFields
{
    public const string Sku = nameof(Product.Sku);

    public const string Name = nameof(Product.Name);

    public const string CostPrice = nameof(Product.CostPrice);

    public const string SellingPrice = nameof(Product.SellingPrice);

    public const string IsActive = nameof(Product.IsActive);
}