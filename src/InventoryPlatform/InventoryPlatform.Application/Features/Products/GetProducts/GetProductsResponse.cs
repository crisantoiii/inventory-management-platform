namespace InventoryPlatform.Application.Features.Products.GetProducts;

public sealed record GetProductsResponse(
    int Id,
    string Sku,
    string Name,
    int CategoryId,
    int UnitId,
    string CategoryName,
    string UnitName,
    decimal QuantityOnHand,
    decimal CostPrice,
    decimal SellingPrice,
    bool IsActive);