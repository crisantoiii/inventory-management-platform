namespace InventoryPlatform.Application.Features.Products.DeactivateProduct;

public sealed record DeactivateProductResponse(
    int Id,
    string Name,
    bool IsActive);