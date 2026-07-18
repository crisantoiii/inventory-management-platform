namespace InventoryPlatform.Application.Features.Products.GetProduct;

public sealed record GetProductResponse(
    int Id,
    string Sku,
    string Name,
    string Unit,
    decimal CostPrice,
    decimal SellingPrice,
    string? Barcode,
    string? Description,
    bool IsActive);