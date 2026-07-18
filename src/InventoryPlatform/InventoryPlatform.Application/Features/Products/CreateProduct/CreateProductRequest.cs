namespace InventoryPlatform.Application.Features.Products.CreateProduct;

public sealed record CreateProductRequest(
    string Sku,
    string Name,
    string Unit,
    decimal CostPrice,
    decimal SellingPrice,
    string? Barcode,
    string? Description);