namespace InventoryPlatform.Application.Features.Products.GetProducts;

public sealed record GetProductsRequest(
    string? Search = null);