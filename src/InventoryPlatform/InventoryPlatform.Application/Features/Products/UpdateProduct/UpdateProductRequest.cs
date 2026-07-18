namespace InventoryPlatform.Application.Features.Products.UpdateProduct;

public sealed record UpdateProductRequest(
    int Id,
    string Name,
    string Unit,
    decimal CostPrice,
    decimal SellingPrice,
    string? Barcode,
    string? Description);