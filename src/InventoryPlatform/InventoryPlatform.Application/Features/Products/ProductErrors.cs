using InventoryPlatform.Shared.Results;

namespace InventoryPlatform.Application.Features.Products;

public static class ProductErrors
{
    public static readonly Error NotFound =
        new(
            "Product.NotFound",
            "Product not found.");

    public static readonly Error DuplicateSku =
        new(
            "Product.DuplicateSku",
            "A product with the same SKU already exists.");
}