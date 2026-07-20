using InventoryPlatform.Domain.Entities;

namespace InventoryPlatform.Application.Features.Products.GetProducts;

public static class CategoriesSortFields
{
    public const string Name = nameof(Product.Name);

    public const string IsActive = nameof(Product.IsActive);
}