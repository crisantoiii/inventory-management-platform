namespace InventoryPlatform.Application.Features.Products.UpdateProduct;

public sealed record UpdateProductRequest(
    int Id,
    string Name,
    int CategoryId,
    int UnitId,
    decimal QuantityOnHand,
    decimal CostPrice,
    decimal SellingPrice,
    string? Barcode,
    string? Description);