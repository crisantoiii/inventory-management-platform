namespace InventoryPlatform.Application.Features.Products.ActivateProduct;

public sealed record ActivateProductResponse(
    int Id,
    string Name,
    bool IsActive);