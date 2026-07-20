namespace InventoryPlatform.Application.Features.Categories.DeactivateCategory;

public sealed record DeactivateCategoryResponse(
    int Id,
    string Name,
    bool IsActive);