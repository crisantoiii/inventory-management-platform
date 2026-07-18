namespace InventoryPlatform.Application.Features.Products.GetProducts;

public sealed record GetProductsResponse(
    int Id,
    string Sku,
    string Name,
    decimal CostPrice,
    decimal SellingPrice,
    bool IsActive);