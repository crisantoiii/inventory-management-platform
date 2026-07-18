namespace InventoryPlatform.Application.Features.Products.CreateProduct;

public sealed record CreateProductResponse(
    int Id,
    string Sku,
    string Name);