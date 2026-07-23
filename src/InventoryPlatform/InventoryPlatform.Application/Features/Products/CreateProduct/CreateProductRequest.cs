namespace InventoryPlatform.Application.Features.Products.CreateProduct;

public sealed record CreateProductRequest(
    string Sku,
    string Name,
    int UnitId,
    int CategoryId,
    decimal QuantityOnHand,
    decimal CostPrice,
    decimal SellingPrice,
    string? Barcode,
    string? Description);